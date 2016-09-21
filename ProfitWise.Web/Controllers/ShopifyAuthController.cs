using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OAuthSandbox.Controllers;
using OAuthSandbox.Models;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Web.Shopify;

namespace ProfitWise.Web.Controllers
{
    public class ShopifyAuthController : Controller
    {
        private readonly IAuthenticationManager _authenticationManager;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationSignInManager _signInManager;
        private readonly IShopifyCredentialService _credentialService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IPushLogger _logger;

        public ShopifyAuthController(
                IAuthenticationManager authenticationManager, 
                ApplicationUserManager userManager,
                ApplicationSignInManager signInManager,
                IShopifyCredentialService credentialService,
                ApplicationDbContext dbContext,
                IPushLogger logger)
        {
            _authenticationManager = authenticationManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _credentialService = credentialService;
            _dbContext = dbContext;
            _logger = logger;
        }


        // GET: /ShopifyAuth/Index
        [HttpGet]
        [AllowAnonymous]
        public ActionResult UnauthorizedAccess(string returnUrl)
        {
            return View(new UnauthorizedAccessModel {  ReturnUrl =  returnUrl});
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UnauthorizedAccess(string returnUrl, string shopname)
        {
            return RedirectToAction("Login", new { @returnUrl = returnUrl, shop = shopname });
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
                // The User exists already - good! Even so, copy the latest set of Claims to Persistence
                await RefreshIdentityClaimsToPersistence(externalLoginInfo);
                return RedirectToLocal(returnUrl);
            }

            return await CreateNewUserAndSignIn(returnUrl, externalLoginInfo);
        }

        private async Task<ActionResult> CreateNewUserAndSignIn(string returnUrl, ExternalLoginInfo externalLoginInfo)
        {
            // It appears that the User does not exist yet
            var email = externalLoginInfo.Email;
            var userName = externalLoginInfo.DefaultUserName;
            ApplicationUser user = null;

            using (var transaction = new TransactionScope())
            {
                user = await _userManager.FindByNameAsync(externalLoginInfo.DefaultUserName);

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
                        return View("ExternalLoginFailure");
                    }

                    var addToRoleResult = await _userManager.AddToRoleAsync(user.Id, SecurityConfig.UserRole);
                    if (!addToRoleResult.Succeeded)
                    {
                        _logger.Error(
                            $"Unable to add User {email} / {userName} to {SecurityConfig.UserRole} - " +
                            $"{createUserResult.Errors.StringJoin(";")}");
                        return View("ExternalLoginFailure");
                    }

                    var addLoginResult = await _userManager.AddLoginAsync(user.Id, externalLoginInfo.Login);
                    if (!addLoginResult.Succeeded)
                    {
                        _logger.Error(
                            $"Unable to add Login for User {email} / {userName} - " +
                            $"{addLoginResult.Errors.StringJoin(";")}");
                        return View("ExternalLoginFailure");
                    }
                }

                transaction.Complete();

                _logger.Info($"Created new User for {email} / {userName}");
                _logger.Info($"Added User {email} / {userName} to {SecurityConfig.UserRole}");
                _logger.Info($"Added Login for User {email} / {userName}");
            }

            // Save the Shopify Domain and Access Token to Persistence
            await RefreshIdentityClaimsToPersistence(externalLoginInfo);

            // Finally Sign-in Manager
            await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            return RedirectToLocal(returnUrl);
        }

        private async Task RefreshIdentityClaimsToPersistence(ExternalLoginInfo externalLoginInfo)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(externalLoginInfo.DefaultUserName);
            var newDomainClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyDomainClaimExternal);
            var newAccessTokenClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyOAuthAccessTokenClaimExternal);

            _credentialService.SetUserCredentials(user.Id, newDomainClaim.Value, newAccessTokenClaim.Value);
            _logger.Info($"Successfully refreshed Identity Claims for User {externalLoginInfo.DefaultUserName}");
        }

        // POST: /ShopifyAuth/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("UnauthorizedAccess", "ShopifyAuth");
        }

        // GET: /ShopifyAuth/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
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

