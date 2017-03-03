using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Web.Shopify;
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
                ShopRepository shopRepository)
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

                // Looks like the first cookie died, was corrupted, etc. We'll ask the User to refresh their browser.
                return RedirectToAction("ExternalLoginFailure", new { returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login
            var externalLoginCookieResult =
                    await _signInManager.ExternalSignInAsync(externalLoginInfo, isPersistent: false);

            if (externalLoginCookieResult == SignInStatus.Success)
            {
                // Found a login, update the Claims and carry on!
                return await UpdateAccount(returnUrl, externalLoginInfo);
            }
            else
            {
                // If no login, then provision a brand new ProfitWise account
                return await CreateNewAccount(returnUrl, externalLoginInfo);
            }
        }

        private async Task<ActionResult> UpdateAccount(
                        string returnUrl, ExternalLoginInfo externalLoginInfo)
        {
            var user = await _userManager.FindByNameAsync(externalLoginInfo.DefaultUserName);
            if (_shopSynchronizationService.IsShopExistingButDisabled(user.Id))
            {
                _logger.Error($"Attempt to login to disabled PwShop by {externalLoginInfo.DefaultUserName}");
                return RedirectToAction("SevereAuthorizationFailure", new { returnUrl });
            }
            _logger.Info($"Existing User {externalLoginInfo.DefaultUserName} has just authenticated");

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // The User exists already - good! Even so, copy the latest set of Claims to Persistence
                SaveExternalCredentialClaims(externalLoginInfo);
                transaction.Complete();
            }

            return RedirectToLocal(returnUrl);
        }

        private async Task<ApplicationUser> CreateUser(ExternalLoginInfo externalLoginInfo)
        {
            var email = externalLoginInfo.Email;
            var userName = externalLoginInfo.DefaultUserName;
            
            // Atomically creates new User
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var user = new ApplicationUser { UserName = userName, Email = email, };
                if (!await _userService.CreateNewUser(user, externalLoginInfo.Login))
                {
                    return null;
                }

                SaveExternalCredentialClaims(externalLoginInfo);
                _logger.Info($"Created new User for {email}/{userName} - added to {SecurityConfig.UserRole} Roles and added Login");
                transaction.Complete();

                return user;
            }
        }

        private async Task<int> CreateShop(ExternalLoginInfo externalLoginInfo, string userId)
        {
            int shopId;
            using (var transaction = _shopSynchronizationService.InitiateTransaction())
            {
                var shopSerialised =
                    externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyShopSerializedExternal);
                var shop = Shop.MakeFromJson(shopSerialised.Value);
                shopId = _shopSynchronizationService.CreateShop(userId, shop);

                transaction.Commit();
            }
            return shopId;
        }

        private async Task<ActionResult> CreateNewAccount(string returnUrl, ExternalLoginInfo externalLoginInfo)
        {
            // Atomically creates new User
            var user = await CreateUser(externalLoginInfo);
            if (user == null)
            {
                return RedirectToAction("ExternalLoginFailure", new { returnUrl });
            }

            // Atomically creates new Shop, new Batch State and schedules Shop Refresh
            var shopId = await CreateShop(externalLoginInfo, user.Id);

            // Create ProfitWise subscription and save
            var verifyUrl = GlobalConfig.BaseUrl + "/ShopifyAuth/VerifyBilling";
            var charge = _billingService.UpsertCharge(user.Id, verifyUrl);
            _shopRepository.UpdateShopifyRecurringChargeId(shopId, charge.id);

            // Issue cookies and redirect for Shopify Charge approval
            await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            return View("ChargeConfirm", 
                            new ChargeConfirmModel() { ConfirmationUrl = charge.confirmation_url});
        }

        private void SaveExternalCredentialClaims(ExternalLoginInfo externalLoginInfo)
        {
            // Matches ASP.NET User with the User identified by the external cookie
            var user = _userManager.FindByName(externalLoginInfo.DefaultUserName);
            _credentialService.SetUserCredentials(user.Id, externalLoginInfo);
            _logger.Info($"Successfully refreshed Identity Claims for User {externalLoginInfo.DefaultUserName}");
        }


        // TODO => try adding charge_id
        public ActionResult VerifyBilling()
        {
            var userId = HttpContext.User.ExtractUserId();
            var charge = _billingService.RetrieveCharge(userId);
            var valid = charge.status == "accepted";

            // If charge is not valid, then what...?
            var shop = _shopRepository.RetrieveByUserId(userId);
            _shopRepository.UpdateIsBillingValid(shop.PwShopId, valid);
            _hangFireService.TriggerInitialShopRefresh(userId);

            return Redirect("~");
        }



        // Error page
        [HttpGet]
        [AllowAnonymous]
        public ActionResult UnauthorizedAccess(string returnUrl)
        {           
            return AuthorizationProblem(
                returnUrl, "Unauthorized Access", "It appears you are not logged into ProfitWise.");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure(string returnUrl)
        {
            return AuthorizationProblem(
                returnUrl, "External Login Failure", 
                "It appears that something went wrong while authorizing your Shopify Account.");            
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult AccessTokenRefresh(string returnUrl)
        {
            return AuthorizationProblem(
                returnUrl, "Refresh Shopify Access",
                "It appears your Shopify Access has expired or is invalid.");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SevereAuthorizationFailure(string returnUrl)
        {
            return AuthorizationProblem(
                returnUrl, "Authorization Failure",
                "Something went wrong while attempting to authorize your Shopify account.");
        }        

        private ActionResult AuthorizationProblem(string returnUrl, string title, string message)
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


