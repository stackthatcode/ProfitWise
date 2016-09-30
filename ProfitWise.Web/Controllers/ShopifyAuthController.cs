using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ProfitWise.Web.Models;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Model;

namespace ProfitWise.Web.Controllers
{
    public class ShopifyAuthController : Controller
    {
        private readonly IAuthenticationManager _authenticationManager;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationSignInManager _signInManager;
        private readonly IShopifyCredentialService _credentialService;
        private readonly IPushLogger _logger;

        public ShopifyAuthController(
                IAuthenticationManager authenticationManager, 
                ApplicationUserManager userManager,
                ApplicationSignInManager signInManager,
                IShopifyCredentialService credentialService,
                IPushLogger logger)
        {
            _authenticationManager = authenticationManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _credentialService = credentialService;
            _logger = logger;
        }



        [AllowAnonymous]
        public ActionResult Login(string shop, string returnUrl)
        {
            var correctedShopName = shop.Replace(".myshopify.com", "");

            // Request a redirect to the external login provider
            return new ShopifyChallengeResult(
                Url.Action("ExternalLoginCallback", "ShopifyAuth", new { ReturnUrl = returnUrl }), null, correctedShopName);
        }
        
        // GET: /ShopifyAuth/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            // External Login Info is contained in the OWIN-issued cookie, and has information from the 
            // external call to the Shop's OAuth service.
            var externalLoginInfo = await _authenticationManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                _logger.Warn("Unable to retrieve ExternalLoginInfo from Authentication Manager");
                // Looks like the first cookie died, was corrupted, etc. We'll ask the User to refresh their browser.
                return View("ExternalLoginFailure");
            }

            // Sign in the user with this external login provider if the user already has a login
            var externalLoginCookieResult =
                await _signInManager.ExternalSignInAsync(externalLoginInfo, isPersistent: false);

            if (externalLoginCookieResult == SignInStatus.Success)
            {
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

                    var createUserResult = await _userManager.CreateAsync(user);
                    if (!createUserResult.Succeeded)
                    {
                        _logger.Error(
                            $"Unable to create new User for {email} / {userName} - " +
                            $"{createUserResult.Errors.StringJoin(";")}");
                        return ExternalLoginFailure(returnUrl);
                    }

                    var addToRoleResult = await _userManager.AddToRoleAsync(user.Id, SecurityConfig.UserRole);
                    if (!addToRoleResult.Succeeded)
                    {
                        _logger.Error(
                            $"Unable to add User {email} / {userName} to {SecurityConfig.UserRole} - " +
                            $"{createUserResult.Errors.StringJoin(";")}");
                        return ExternalLoginFailure(returnUrl);
                    }

                    var addLoginResult = await _userManager.AddLoginAsync(user.Id, externalLoginInfo.Login);
                    if (!addLoginResult.Succeeded)
                    {
                        _logger.Error(
                            $"Unable to add Login for User {email} / {userName} - " +
                            $"{addLoginResult.Errors.StringJoin(";")}");
                        return ExternalLoginFailure(returnUrl);
                    }
                }

                // Save the Shopify Domain and Access Token to Persistence
                await PushCookieClaimsToPersistence(externalLoginInfo);

                transaction.Complete();

                _logger.Info($"Created new User for {email} / {userName}");
                _logger.Info($"Added User {email} / {userName} to {SecurityConfig.UserRole}");
                _logger.Info($"Added Login for User {email} / {userName}");
            }

            // Finally Sign-in Manager
            await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            return RedirectToLocal(returnUrl);
        }

        private async Task PushCookieClaimsToPersistence(ExternalLoginInfo externalLoginInfo)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(externalLoginInfo.DefaultUserName);

            var domainClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyDomainClaimExternal);
            var accessTokenClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyOAuthAccessTokenClaimExternal);

            _credentialService.SetUserCredentials(user.Id, domainClaim.Value, accessTokenClaim.Value);

            var shopSerialised = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyShopSerializedExternal);

            var shop = new Shop(shopSerialised.Value);

            // TODO - Refresh the Shop

            _logger.Info($"Successfully refreshed Identity Claims for User {externalLoginInfo.DefaultUserName}");
        }

        private async Task CreateProfitWiseShop()
        {
            
        }



        // GET: /ShopifyAuth/UnauthorizedAccess
        [HttpGet]
        [AllowAnonymous]
        public ActionResult UnauthorizedAccess(string returnUrl)
        {
            var shop = returnUrl.ExtractQueryParameter("shop");
            if (shop != null)
            {
                return Login(shop, returnUrl);
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return View(new AuthorizationProblemModel(returnUrl));
            }
        }

        // GET: /ShopifyAuth/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure(string returnUrl)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return View(new AuthorizationProblemModel(returnUrl));
        }

        // GET: /ShopifyAuth/AccessTokenRefresh
        [AllowAnonymous]
        public ActionResult AccessTokenRefresh(string returnUrl)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return View(new AuthorizationProblemModel(returnUrl));
        }

        // GET: /ShopifyAuth/AuthorizationFailure
        [AllowAnonymous]
        public ActionResult SevereAuthorizationFailure(string returnUrl)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return View(new AuthorizationProblemModel(returnUrl));
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Dashboard", "UserMain");
        }
    }
}

