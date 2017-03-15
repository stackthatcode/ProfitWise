using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ProfitWise.Data.Configuration;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using ProfitWise.Data.Utility;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Utilities.Security;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Web.Json;
using Push.Foundation.Web.Shopify;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using ShopifySharp;
using ShopifySharp.Enums;


namespace ProfitWise.Web.Controllers
{
    public class ShopifyAuthController : Controller
    {
        private readonly IAuthenticationManager _authenticationManager;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationSignInManager _signInManager;
        private readonly IShopifyCredentialService _credentialService;
        private readonly ShopOrchestrationService _shopOrchestrationService;
        private readonly OwinUserService _userService;
        private readonly ShopRepository _shopRepository;
        private readonly IPushLogger _logger;
        private readonly HmacCryptoService _hmacCryptoService;
        private readonly ApiRepositoryFactory _factory;    

        public ShopifyAuthController(
                IAuthenticationManager authenticationManager, 
                ApplicationUserManager userManager,
                ApplicationSignInManager signInManager,
                IShopifyCredentialService credentialService,
                ShopOrchestrationService shopSynchronizationService,
                OwinUserService userService,
                IPushLogger logger, 
                ShopRepository shopRepository, 
                HmacCryptoService hmacCryptoService, 
                ApiRepositoryFactory factory)
        {
            _authenticationManager = authenticationManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _credentialService = credentialService;
            _userService = userService;
            _logger = logger;
            _shopRepository = shopRepository;
            _hmacCryptoService = hmacCryptoService;
            _factory = factory;
            _shopOrchestrationService = shopSynchronizationService;
        }


        // Shopify OAuth authentication & authorization flow
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
        public ActionResult LoginAlt(string shop, string returnUrl)
        {
            // First strip everything off so we can standardize
            var correctedShopName = shop.ShopNameFromDomain();
            
            // Next create the Shop URL
            // TODO - move to our Extensions methods thingy
            var shopUrl = $"https://{correctedShopName}.myshopify.com";

            // If the return url is empty, let's fix that - do we need Return URL...??
            //returnUrl = returnUrl ?? $"{GlobalConfig.BaseUrl}/?shop={shop}";

            var redirectUrl = GlobalConfig.BuildUrl("/ShopifyAuth/ShopifyReturn");

            var scopes = new List<ShopifyAuthorizationScope>()
            {
                ShopifyAuthorizationScope.ReadOrders,
                ShopifyAuthorizationScope.ReadProducts,
            };

            var authUrl = 
                ShopifyAuthorizationService.BuildAuthorizationUrl(
                    scopes, shopUrl, ProfitWiseConfiguration.Settings.ShopifyApiKey, redirectUrl);

            return Redirect(authUrl.ToString());
        }


        [AllowAnonymous]
        public async Task<ActionResult> ShopifyReturn(string returnUrl)
        {
            // Attempt to complete Shopify Authentication
            var profitWiseSignIn = await CompleteShopifyAuth();
            if (profitWiseSignIn == null)
            {
                return GlobalConfig.Redirect(AuthConfig.ExternalLoginFailureUrl);
            }
            
            // Create User
            ApplicationUser user = await UpsertAspNetUserAcct(profitWiseSignIn);
            if (user == null)
            {
                return GlobalConfig.Redirect(AuthConfig.ExternalLoginFailureUrl, returnUrl);
            }

            // Create/Update the Shop
            PwShop shop = UpsertProfitWiseShop(profitWiseSignIn, user);
            if (shop == null)
            {
                return GlobalConfig.Redirect(AuthConfig.SevereAuthorizationFailureUrl, returnUrl);
            }

            return UpsertBilling(user, returnUrl);
        }
        
        private async Task<ProfitWiseSignIn> CompleteShopifyAuth()
        {
            string code = Request.QueryString["code"];
            string shopDomain = Request.QueryString["shop"];
            var apikey = ProfitWiseConfiguration.Settings.ShopifyApiKey;
            var apisecret = ProfitWiseConfiguration.Settings.ShopifyApiSecret;

            try
            {
                var accessToken = 
                    await ShopifyAuthorizationService
                        .Authorize(code, shopDomain, apikey, apisecret);

                var credentials = ShopifyCredentials.Build(shopDomain, accessToken);
                var shopApiRepository = _factory.MakeShopApiRepository(credentials);
                var shopFromShopify = shopApiRepository.Retrieve();

                return new ProfitWiseSignIn
                {
                    AccessToken = accessToken,
                    Shop = shopFromShopify,
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }

        private async Task<ApplicationUser> UpsertAspNetUserAcct(ProfitWiseSignIn signin)
        {
            // FYI, UserName == Domain
            try
            {
                var existingUser = await _userManager.FindByNameAsync(signin.AspNetUserName);
                if (existingUser == null)
                {
                    using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var newUser = new ApplicationUser
                        {
                            UserName = signin.AspNetUserName,
                            Email = signin.Shop.Email,
                        };
                        var login = new UserLoginInfo("Shopify", signin.Shop.Id.ToString());
                        if (!await _userService.CreateNewUser(newUser, login))
                        {
                            _logger.Warn($"UserService.CreateNewUser failed for {signin.AspNetUserName}");
                            return null;
                        }

                        // Save the Domain and Access Token to the ASP.NET Claims
                        _credentialService.SetUserCredentials(newUser.Id, signin.Shop.Domain, signin.AccessToken);
                        _logger.Info($"Created new User, Shopify Login and Claims for {signin.AspNetUserName}");

                        // Issue the OWIN cookie
                        await _signInManager.SignInAsync(newUser, isPersistent: false, rememberBrowser: false);

                        transaction.Complete();
                        return newUser;
                    }
                }
                else
                {
                    await _signInManager.SignInAsync(existingUser, isPersistent: false, rememberBrowser: false);
                    _credentialService.SetUserCredentials(existingUser.Id, signin.Shop.Domain, signin.AccessToken);
                    _logger.Info($"Updated Claims  for {signin.AspNetUserName}");
                    return existingUser;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }

        private PwShop UpsertProfitWiseShop(ProfitWiseSignIn signIn, ApplicationUser user)
        {
            try
            {
                var pwShop = _shopRepository.RetrieveByUserId(user.Id);
                var credentials = ShopifyCredentials.Build(signIn.Shop.Domain, signIn.AccessToken);

                // Create the Shop database records
                if (pwShop == null)
                {
                    using (var transaction = _shopOrchestrationService.InitiateTransaction())
                    {
                        pwShop = _shopOrchestrationService.CreateShop(user.Id, signIn.Shop, credentials);
                        transaction.Commit();
                    }
                }
                else
                {
                    _shopOrchestrationService.UpdateShop(user.Id, signIn.Shop.Currency, signIn.Shop.TimeZone);
                }

                // Ensure that the Uninstall Webhook is in place
                _shopOrchestrationService.UpsertUninstallWebhook(credentials);
                return pwShop;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }

        }



        [AllowAnonymous]
        [Obsolete]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            // External Login Info is contained in the OWIN-issued cookie, and has information from the 
            // external call to the Shop's OAuth service.
            var externalLoginInfo = await _authenticationManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                _logger.Error("Unable to retrieve ExternalLoginInfo from Authentication Manager");
                return GlobalConfig.Redirect(AuthConfig.ExternalLoginFailureUrl, returnUrl);
            }

            // Sign in the user with this external login provider if the user already has a login
            ApplicationUser user;
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                user = await UpsertAspNetUserAcct(externalLoginInfo);
                transaction.Complete();
            }
            if (user == null)
            {
                return GlobalConfig.Redirect(AuthConfig.ExternalLoginFailureUrl, returnUrl);
            }

            // Create/Update the Shop
            PwShop shop = UpsertProfitWiseShop(externalLoginInfo, user);
            if (shop == null)
            {
                return GlobalConfig.Redirect(AuthConfig.SevereAuthorizationFailureUrl, returnUrl);
            }

            // Handle Billing
            return UpsertBilling(user, returnUrl);
        }

        [Obsolete]
        private async Task<ApplicationUser> UpsertAspNetUserAcct(ExternalLoginInfo externalLoginInfo)
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

        [Obsolete]
        private void SaveExternalInfoToClaims(ApplicationUser user, ExternalLoginInfo externalLoginInfo)
        {
            // Matches ASP.NET User with the User identified by the external cookie
            _credentialService.SetUserCredentials(user.Id, externalLoginInfo);
            _logger.Info($"Successfully refreshed Identity Claims for User {externalLoginInfo.DefaultUserName}");
        }

        [Obsolete]
        private PwShop UpsertProfitWiseShop(ExternalLoginInfo externalLoginInfo, ApplicationUser user)
        {
            // TODO - add naughty Shop blacklist to configuration file

            var pwShop = _shopRepository.RetrieveByUserId(user.Id);
            var shopJson = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyShopSerializedExternal);
            var shop = Shop.MakeFromJson(shopJson.Value);
            var credentials = CredentialFactory.Make(externalLoginInfo, user);

            // Create the Shop database records
            if (pwShop == null)
            {
                using (var transaction = _shopOrchestrationService.InitiateTransaction())
                {
                    pwShop = _shopOrchestrationService.CreateShop(user.Id, shop, credentials);
                    transaction.Commit();
                }
            }
            else
            {
                _shopOrchestrationService.UpdateShop(user.Id, shop.Currency, shop.TimeZone);
            }

            // Ensure that the Uninstall Webhook is in place
            _shopOrchestrationService.UpsertUninstallWebhook(credentials);
            return pwShop;
        }
        


        private ActionResult UpsertBilling(ApplicationUser user, string returnUrl)
        {
            var charge = _shopOrchestrationService.SyncAndRetrieveCurrentCharge(user.Id);

            if (charge != null && charge.LastStatus == ChargeStatus.Pending)
            {
                // Redirect for Shopify Charge approval
                return View("ChargeConfirm",
                    new ChargeConfirmModel() { ConfirmationUrl = charge.ConfirmationUrl });
            }            
            if (charge == null || charge.LastStatus.SystemMustCreateNewCharge())
            {
                // Create ProfitWise subscription and save
                var verifyUrl = GlobalConfig.BaseUrl + "/ShopifyAuth/VerifyBilling";
                var newCharge = _shopOrchestrationService.CreateCharge(user.Id, verifyUrl);

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
            var chargeAccepted =_shopOrchestrationService.VerifyChargeAndScheduleRefresh(userId);            
            if (chargeAccepted)
            {           
                return Redirect("~");
            }
            else
            {
                return BillingDeclined();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult BillingDeclined()
        {
            AuthConfig.GlobalSignOut(_signInManager);
            return View("BillingDeclined");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Uninstall(UninstallWebhook message)
        {
            _logger.Info($"App Uninstall Webhook invocation Shopify Shop Id: {message.id}");
            var shopifyHash = Request.Headers["X-Shopify-Hmac-Sha256"];
            Request.InputStream.Position = 0;
            var rawRequest = new StreamReader(Request.InputStream).ReadToEnd();
            var verifyHash = _hmacCryptoService.ToBase64EncodedSha256(rawRequest);

            if (shopifyHash != verifyHash)
            {
                _logger.Error($"Hash Verification failure {shopifyHash}/{verifyHash}");
            }

            _shopOrchestrationService.UninstallShop(message.id);
            return JsonNetResult.Success();
        }



        // Authorization error pages
        [HttpGet]
        [AllowAnonymous]
        public ActionResult UnauthorizedAccess(string returnUrl)
        {
            return AuthorizationProblem(
                returnUrl, "Unauthorized Access", "It appears you are not logged into ProfitWise.",
                showLoginLink: true);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure(string returnUrl)
        {
            var msg = "It appears that something went wrong while authorizing your Shopify Account.";
            return AuthorizationProblem(returnUrl, "External Login Failure", msg, showLoginLink: true);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult AccessTokenRefresh(string returnUrl)
        {
            var msg = "It appears your Shopify Access has expired or is invalid.";
            return AuthorizationProblem(returnUrl, "Refresh Shopify Access", msg, showLoginLink: true);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SevereAuthorizationFailure(string returnUrl)
        {
            var msg = "Something went wrong while attempting to authorize your Shopify account.";
            return AuthorizationProblem(returnUrl, "Authorization Failure", msg, showLoginLink: true);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult BillingProblem(string returnUrl)
        {
            var msg = "Something went wrong while attempting to bill your ProfitWise account. " +
                    "Please contact our support for more information.";
            return AuthorizationProblem(returnUrl, "Billing Problem", msg);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult BillingIncomplete(string returnUrl)
        {
            var msg = "It appears that your ProfitWise billing hasn't been set up correctly.";
            return AuthorizationProblem(returnUrl, "Billing Incomplete", msg, showLoginLink: true);
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
                    Message = message,
                    ShowLoginLink = showLoginLink,
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



