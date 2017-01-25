using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
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
        private readonly UserService _userService;
        private readonly IPushLogger _logger;
        

        public ShopifyAuthController(
                IAuthenticationManager authenticationManager, 
                ApplicationUserManager userManager,
                ApplicationSignInManager signInManager,
                IShopifyCredentialService credentialService,
                UserService userService,
                IPushLogger logger, 
                ShopSynchronizationService shopSynchronizationService)
        {
            _authenticationManager = authenticationManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _credentialService = credentialService;
            _userService = userService;
            _logger = logger;
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
                var user = await _userManager.FindByNameAsync(externalLoginInfo.DefaultUserName);                
                if (_shopSynchronizationService.IsShopExistingButDisabled(user.Id))
                {
                    _logger.Error($"Attempt to login to disabled PwShop by {externalLoginInfo.DefaultUserName}");
                    return RedirectToAction("SevereAuthorizationFailure", new { returnUrl });
                }

                _logger.Info($"Existing User {externalLoginInfo.DefaultUserName} has just authenticated");

                using (var transaction = new TransactionScope())
                {
                    // The User exists already - good! Even so, copy the latest set of Claims to Persistence
                    await PushCookieClaimsToPersistence(externalLoginInfo);
                    transaction.Complete();
                }

                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If no login, then provision a brand new ProfitWise account
                return await CreateNewUserAndSignIn(returnUrl, externalLoginInfo);
            }
        }

        private async Task<ActionResult> 
                    CreateNewUserAndSignIn(string returnUrl, ExternalLoginInfo externalLoginInfo)
        {
            // It appears that the User does not exist yet
            var email = externalLoginInfo.Email;
            var userName = externalLoginInfo.DefaultUserName;
            ApplicationUser user = null;

            using (var transaction = new TransactionScope())
            {
                user = await _userManager.FindByNameAsync(userName);

                // If User doesn't exist, create a new one
                if (user == null)
                {
                    user = new ApplicationUser {UserName = userName, Email = email,};
                    if (!await _userService.CreateNewUser(user, externalLoginInfo.Login))
                    {
                        return RedirectToAction("ExternalLoginFailure", new { returnUrl });
                    }
                }

                // Save the Shopify Domain and Access Token to Persistence
                await PushCookieClaimsToPersistence(externalLoginInfo);
                transaction.Complete();
            }

            _logger.Info($"Created new User for {email}/{userName} - added to {SecurityConfig.UserRole} Roles and added Login");

            // Finally Sign-in Manager
            await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            return RedirectToLocal(returnUrl);
        }

        private async Task PushCookieClaimsToPersistence(ExternalLoginInfo externalLoginInfo)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(externalLoginInfo.DefaultUserName);

            var domainClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyDomainClaimExternal);
            var accessTokenClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyOAuthAccessTokenClaimExternal);
            var shopSerialised = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyShopSerializedExternal);

            // Push the Domain and the Access Token
            _credentialService.SetUserCredentials(user.Id, domainClaim.Value, accessTokenClaim.Value);

            // Update the Shop with the latest serialized Shop from the OAuth handshake
            var shop = new Shop(shopSerialised.Value);
            _shopSynchronizationService.RefreshShop(user.Id, shop);

            _logger.Info($"Successfully refreshed Identity Claims for User {externalLoginInfo.DefaultUserName}");
        }
        
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect(GlobalConfig.BaseUrl + returnUrl.ExtractQueryString());
        }



        // Error pages...

        [HttpGet]
        [AllowAnonymous]
        public ActionResult UnauthorizedAccess(string returnUrl)
        {           
            return AuthorizationProblem(
                returnUrl, "Unauthorized Access", "It appears you are not logged into ProfitWise.");
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure(string returnUrl)
        {
            return AuthorizationProblem(
                returnUrl, "External Login Failure", 
                "It appears that something went wrong while authorizing your Shopify Account.");            
        }

        [AllowAnonymous]
        public ActionResult AccessTokenRefresh(string returnUrl)
        {
            return AuthorizationProblem(
                returnUrl, "Refresh Shopify Access",
                "It appears your Shopify Access has expired or is invalid.");
        }

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
    }
}


