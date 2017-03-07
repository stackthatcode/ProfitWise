using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Utilities.Security;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Web.Json;
using Push.Foundation.Web.Shopify;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;


namespace ProfitWise.Web.Controllers
{
    public class ShopifyAuthController : Controller
    {
        private readonly IAuthenticationManager _authenticationManager;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationSignInManager _signInManager;
        private readonly IShopifyCredentialService _credentialService;
        private readonly ShopSynchronizationService _shopSynchronizationService;
        private readonly OwinUserService _userService;
        private readonly HangFireService _hangFireService;
        private readonly BillingService _billingService;
        private readonly ShopRepository _shopRepository;
        private readonly IPushLogger _logger;
        private readonly HmacCryptoService _hmacCryptoService;      

        public ShopifyAuthController(
                IAuthenticationManager authenticationManager, 
                ApplicationUserManager userManager,
                ApplicationSignInManager signInManager,
                IShopifyCredentialService credentialService,
                ShopSynchronizationService shopSynchronizationService,
                OwinUserService userService,
                HangFireService hangFireService,
                BillingService billingService,
                IPushLogger logger, 
                ShopRepository shopRepository, 
                HmacCryptoService hmacCryptoService)
        {
            _authenticationManager = authenticationManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _credentialService = credentialService;
            _userService = userService;
            _hangFireService = hangFireService;
            _billingService = billingService;
            _logger = logger;
            _shopRepository = shopRepository;
            _hmacCryptoService = hmacCryptoService;
            _shopSynchronizationService = shopSynchronizationService;
        }


        [AllowAnonymous]
        public ActionResult Login(string shop, string returnUrl)
        {
            var correctedShopName = shop.Replace(".myshopify.com", "");
            returnUrl = returnUrl ?? $"{GlobalConfig.BaseUrl}/?shop={shop}";

            // Request a redirect to the external login provider
            return new ShopifyChallengeResult(
                Url.Action("ExternalLoginCallback", "ShopifyAuth", new { ReturnUrl = returnUrl }), null, correctedShopName);
        }
        
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            // External Login Info is contained in the OWIN-issued cookie, and has information from the 
            // external call to the Shop's OAuth service.
            var externalLoginInfo = await _authenticationManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                _logger.Error("Unable to retrieve ExternalLoginInfo from Authentication Manager");
                return RedirectToAction("ExternalLoginFailure", new { returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login
            ApplicationUser user;
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                user = await UpsertAspNetUserAndSignIn(externalLoginInfo);
                transaction.Complete();
            }
            if (user == null)
            {
                return RedirectToAction("ExternalLoginFailure", new {returnUrl});
            }

            // Create/Update the Shop
            PwShop shop = UpsertShop(externalLoginInfo, user);
            if (shop == null)
            {
                return RedirectToAction("SevereAuthorizationFailure", new {returnUrl});
            }

            // Handle Billing
            return UpsertBilling(user, returnUrl);
        }

        private async Task<ApplicationUser> UpsertAspNetUserAndSignIn(ExternalLoginInfo externalLoginInfo)
        {            
            var externalLoginCookieResult =
                    await _signInManager.ExternalSignInAsync(externalLoginInfo, isPersistent: false);

            if (externalLoginCookieResult != SignInStatus.Success)
            {
                var email = externalLoginInfo.Email;
                var userName = externalLoginInfo.DefaultUserName;
                var newUser = new ApplicationUser {UserName = userName, Email = email,};

                if (!await _userService.CreateNewUser(newUser, externalLoginInfo.Login))
                {
                    _logger.Warn($"UserService.CreateNewUser failed for {email}/{userName}");
                    return null;
                }

                SaveExternalInfoToClaims(newUser, externalLoginInfo);
                await _signInManager.SignInAsync(newUser, isPersistent: false, rememberBrowser: false);

                _logger.Info($"Created new User, Shopify Login and Claims for {email}/{userName}");
                
                return newUser;
            }
            else
            {
                var user = await _userManager.FindByNameAsync(externalLoginInfo.DefaultUserName);
                if (user == null)
                {
                    _logger.Warn($"Unable to find AspNet User for {externalLoginInfo.DefaultUserName}");
                    return null;
                }
                SaveExternalInfoToClaims(user, externalLoginInfo);

                _logger.Info($"Updated Claims  for {externalLoginInfo.Email}/{externalLoginInfo.DefaultUserName}");
                return user;
            }
        }

        private void SaveExternalInfoToClaims(ApplicationUser user, ExternalLoginInfo externalLoginInfo)
        {
            // Matches ASP.NET User with the User identified by the external cookie
            _credentialService.SetUserCredentials(user.Id, externalLoginInfo);
            _logger.Info($"Successfully refreshed Identity Claims for User {externalLoginInfo.DefaultUserName}");
        }

        private PwShop UpsertShop(ExternalLoginInfo externalLoginInfo, ApplicationUser user)
        {
            if (_shopSynchronizationService.ExistsButDisabled(user.Id))
            {
                _logger.Warn($"Attempt to login with disabled Shop by {user.Id}");
            }

            var pwShop = _shopRepository.RetrieveByUserId(user.Id);
            var shopJson = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyShopSerializedExternal);
            var shop = Shop.MakeFromJson(shopJson.Value);

            if (pwShop == null)
            {
                using (var transaction = _shopSynchronizationService.InitiateTransaction())
                {
                    var credentials = MakeCredentials(externalLoginInfo, user);
                    pwShop = _shopSynchronizationService.CreateShop(user.Id, shop, credentials);
                    transaction.Commit();
                }
            }
            else
            {
                _shopSynchronizationService.UpdateShop(user.Id, shop.Currency, shop.TimeZone);
            }
            return pwShop;
        }
        
        private ShopifyCredentials MakeCredentials(ExternalLoginInfo externalLoginInfo, ApplicationUser user)
        {
            var domainClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyDomainClaimExternal);
            var accessTokenClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyOAuthAccessTokenClaimExternal);
            var credentials = new ShopifyCredentials()
            {
                ShopDomain = domainClaim.Value,
                AccessToken = accessTokenClaim.Value,
                ShopOwnerUserId = user.Id
            };
            return credentials;
        }

        private ActionResult UpsertBilling(ApplicationUser user, string returnUrl)
        {
            var charge = _billingService.SyncAndRetrieveCurrentCharge(user.Id);
            if (charge != null && charge.LastStatus == ChargeStatus.Pending)
            {
                // Redirect for Shopify Charge approval
                return View("ChargeConfirm",
                    new ChargeConfirmModel() { ConfirmationUrl = charge.ConfirmationUrl });
            }
            
            if (charge == null || charge.SystemNeedsToCreateNewCharge)
            {
                // Create ProfitWise subscription and save
                var verifyUrl = GlobalConfig.BaseUrl + "/ShopifyAuth/VerifyBilling";
                var newCharge = _billingService.CreateCharge(user.Id, verifyUrl);

                // Redirect for Shopify Charge approval
                return View("ChargeConfirm", 
                    new ChargeConfirmModel() { ConfirmationUrl = newCharge.ConfirmationUrl });
            }
            
            return RedirectToLocal(returnUrl);
        }


        [HttpGet]
        public ActionResult VerifyBilling(string charge_id)
        {
            // Notice: we don't use charge_id, as we rely on our cookies - maybe we should use charge_id?
            // ... or if userId is null, Redirect to Login
            
            var userId = HttpContext.User.ExtractUserId();
            var charge = _billingService.SyncAndRetrieveCurrentCharge(userId);
            var shop = _shopRepository.RetrieveByUserId(userId);
            
            if (charge.IsValid)
            {                
                _shopRepository.UpdateIsBillingValid(shop.PwShopId, true);
                _hangFireService.AddOrUpdateInitialShopRefresh(userId);
                return Redirect("~");
            }
            else
            {
                // TODO - should we remove this Charge Id from ProfitWise
                _shopRepository.UpdateIsBillingValid(shop.PwShopId, false);
                return BillingDeclined();
            }
        }

        [HttpGet]
        public ActionResult BillingDeclined()
        {
            //AuthConfig.GlobalSignOut(_signInManager);
            return View("BillingDeclined");
        }

        [HttpPost]
        public ActionResult Uninstall(UninstallWebhook message)
        {
            var shopifyHash = Request.Headers["X-Shopify-Hmac-Sha256"];
            Request.InputStream.Position = 0;
            var rawRequest = new StreamReader(Request.InputStream).ReadToEnd();
            var verifyHash = _hmacCryptoService.ToBase64EncodedSha256(rawRequest);

            _logger.Debug(shopifyHash);
            _logger.Debug(verifyHash);
            return JsonNetResult.Success();
        }


        // Error pages
        [HttpGet]
        [AllowAnonymous]
        public ActionResult UnauthorizedAccess(string returnUrl)
        {           
            return AuthorizationProblem(
                returnUrl, "Unauthorized Access", "It appears you are not logged into ProfitWise.", 
                showLoginLink:true);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure(string returnUrl)
        {
            var msg = "It appears that something went wrong while authorizing your Shopify Account.";
            return AuthorizationProblem(returnUrl, "External Login Failure",  msg, showLoginLink:true);            
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult AccessTokenRefresh(string returnUrl)
        {
            var msg = "It appears your Shopify Access has expired or is invalid.";
            return AuthorizationProblem(returnUrl, "Refresh Shopify Access", msg, showLoginLink:true);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SevereAuthorizationFailure(string returnUrl)
        {
            var msg = "Something went wrong while attempting to authorize your Shopify account.";
            return AuthorizationProblem(returnUrl, "Authorization Failure", msg, showLoginLink:true);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult BillingProblem(string returnUrl)
        {
            var msg = "Something went wrong while attempting to bill your ProfitWise account. Please contact our support.";
            return AuthorizationProblem(returnUrl, "Billing Problem", msg);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult BillingIncomplete(string returnUrl)
        {
            var msg = "It appears that your ProfitWise billing hasn't been set up correctly.";
            return AuthorizationProblem(returnUrl, "Billing Incomplete", msg, showLoginLink:true);
        }


        private ActionResult AuthorizationProblem(
                string returnUrl, string title, string message, bool showLoginLink = false)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;
           
            var url = GlobalConfig.BaseUrl + $"/ShopifyAuth/Login";

            if (returnUrl != null)
            {
                url += $"?returnUrl={HttpUtility.UrlEncode(returnUrl)}";

                var shop = returnUrl.ExtractQueryParameter("shop");
                if (shop != null)
                {
                    url += $"&shop={shop}";
                }
            }
            var model = 
                new AuthorizationProblemModel(url)
                {
                    Title = title,
                    Message = message
                };

            return View("AuthorizationProblem", model);
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect(GlobalConfig.BaseUrl + returnUrl.ExtractQueryString());
        }
    }
}


