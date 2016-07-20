using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OAuthSandbox.Controllers;
using OAuthSandbox.Models;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Web.Controllers
{
    public class ShopifyAuthController : Controller
    {
        private readonly IAuthenticationManager _authenticationManager;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationSignInManager _signInManager;
        private readonly ShopifyCredentialService _credentialService;

        public ShopifyAuthController(
                IAuthenticationManager authenticationManager, 
                ApplicationUserManager userManager,
                ApplicationSignInManager signInManager, 
                ShopifyCredentialService credentialService)
        {
            _authenticationManager = authenticationManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _credentialService = credentialService;
        }


        // GET: /ShopifyAuth/Index
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index(string returnUrl)
        {
            return View(new ShopifyAuthIndexModel {  ReturnUrl =  returnUrl});
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Refresh(string returnUrl)
        {
            return View(new ShopifyAuthIndexModel { ReturnUrl = returnUrl });
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult EmbeddedAppLogin(string shop, string returnUrl)
        {
            var correctedShopName = shop.Replace(".myshopify.com", "");

            // Request a redirect to the external login provider
            return new ShopifyChallengeResult(
                Url.Action("ExternalLoginCallback", "ShopifyAuth", new { ReturnUrl = returnUrl }), null, correctedShopName);
        }



        // POST: /ShopifyAuth/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string shopname, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ShopifyChallengeResult(
                Url.Action("ExternalLoginCallback", "ShopifyAuth", new { ReturnUrl = returnUrl }), null, shopname);
        }

        // GET: /ShopifyAuth/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var externalLoginInfo = await _authenticationManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                return RedirectToAction("Index", "ShopifyAuth");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalSignInAsync(externalLoginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    await CopyIdentityClaimsToPersistence(externalLoginInfo);

                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");

                // For 2-factor enabled authentication - disabled for now
                //case SignInStatus.RequiresVerification: 
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });

                // SignInManager returns SignInStatus.Failure if an Account does not exist for ExternalLoginInfo
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = externalLoginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = externalLoginInfo.Email });
            }
        }

        // POST: /ShopifyAuth/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get the information about the user from the external login provider
            var externalLoginInfo = await _authenticationManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                return View("ExternalLoginFailure");
            }
            
            // User does not exist in our System? 
            ApplicationUser user = await _userManager.FindByNameAsync(externalLoginInfo.DefaultUserName);

            // If not, create a new Application User
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = externalLoginInfo.DefaultUserName,
                    Email = model.Email
                };
                
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    this.AddErrors(result);
                    return View(model);
                }

                _userManager.AddToRole(user.Id, SecurityConfig.UserRole);
            }

            // Do we have this Login already?
            if (user.Logins
                    .FirstOrDefault(x =>    
                            x.LoginProvider == externalLoginInfo.Login.LoginProvider &&
                            x.ProviderKey == externalLoginInfo.Login.ProviderKey) == null)
            {
                var result = await _userManager.AddLoginAsync(user.Id, externalLoginInfo.Login);
                if (!result.Succeeded)
                {
                    this.AddErrors(result);
                    return View(model);
                }
            }


            // Save the Shopify Domain and Access Token to Persistence
            await CopyIdentityClaimsToPersistence(externalLoginInfo);

            // Finally Sign-in Manager
            await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            return RedirectToLocal(returnUrl);
        }

        private async Task CopyIdentityClaimsToPersistence(ExternalLoginInfo externalLoginInfo)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(externalLoginInfo.DefaultUserName);
            var new_domain_claim =
                externalLoginInfo
                    .ExternalIdentity.Claims.FirstOrDefault(x => x.Type == SecurityConfig.ShopifyDomainClaimExternal);

            var new_access_token_claim =
                externalLoginInfo.ExternalIdentity.Claims.FirstOrDefault(
                    x => x.Type == SecurityConfig.ShopifyOAuthAccessTokenClaimExternal);

            _credentialService.SetUserCredentials(user.Id, new_domain_claim.Value, new_access_token_claim.Value);
        }



        // POST: /ShopifyAuth/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Refresh", "ShopifyAuth");
        }



        // GET: /ShopifyAuth/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }



        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "UserHome");
        }
        #endregion
    }
}

