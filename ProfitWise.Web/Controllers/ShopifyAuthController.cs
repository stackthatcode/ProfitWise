using System;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
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
        private readonly CurrencyService _currencyService;
        private readonly PwShopRepository _pwShopRepository;
        private readonly UserService _userService;
        private readonly IPushLogger _logger;

        private readonly DateTime _defaultStartDateForOrders = new DateTime(2016, 8, 1);


        public ShopifyAuthController(
                IAuthenticationManager authenticationManager, 
                ApplicationUserManager userManager,
                ApplicationSignInManager signInManager,
                IShopifyCredentialService credentialService,
                UserService userService,
                IPushLogger logger, 
                CurrencyService currencyService, 
                PwShopRepository pwShopRepository)
        {
            _authenticationManager = authenticationManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _credentialService = credentialService;
            _userService = userService;
            _logger = logger;
            _currencyService = currencyService;
            _pwShopRepository = pwShopRepository;
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
                return RedirectToAction("ExternalLoginFailure", new { returnUrl });

            }

            // Sign in the user with this external login provider if the user already has a login
            var externalLoginCookieResult =
                await _signInManager.ExternalSignInAsync(externalLoginInfo, isPersistent: false);

            if (externalLoginCookieResult == SignInStatus.Success)
            {
                if (await IsShopExistingAndDisabled(externalLoginInfo))
                {
                    _logger.Error($"Attempt to login to disabled PwShop by {externalLoginInfo.DefaultUserName}");
                    return RedirectToAction("SevereAuthorizationFailure", new {returnUrl});
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

        private async Task<bool> IsShopExistingAndDisabled(ExternalLoginInfo externalLoginInfo)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(externalLoginInfo.DefaultUserName);
            var pwShop = _pwShopRepository.RetrieveByUserId(user.Id);
            return (pwShop != null && pwShop.IsShopEnabled == false);
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
            await RefreshProfitWiseShop(user.Id, shop);

            _logger.Info($"Successfully refreshed Identity Claims for User {externalLoginInfo.DefaultUserName}");
        }

        private async Task RefreshProfitWiseShop(string userId, Shop shop)  
        {
            var currencyId = _currencyService.AbbreviationToCurrencyId(shop.Currency);
            var pwShop = _pwShopRepository.RetrieveByUserId(userId);

            if (pwShop == null)
            {
                var newShop = new PwShop
                {
                    ShopOwnerUserId = userId,
                    CurrencyId = currencyId,
                    ShopifyShopId = shop.Id,
                    StartingDateForOrders = _defaultStartDateForOrders,
                    TimeZone = shop.TimeZone,
                    IsAccessTokenValid = true,
                    IsShopEnabled = true,
                    IsDataLoaded = false,
                };

                newShop.PwShopId = _pwShopRepository.Insert(newShop);

                _logger.Info($"Created new Shop - UserId: {newShop.ShopOwnerUserId}, " +
                             $"CurrencyId: {newShop.CurrencyId}, " +
                             $"ShopifyShopId: {newShop.ShopifyShopId}, " +
                             $"StartingDateForOrders: {newShop.StartingDateForOrders}");
            }
            else
            {
                pwShop.CurrencyId = currencyId;
                pwShop.TimeZone = shop.TimeZone;
                _pwShopRepository.Update(pwShop);

                _pwShopRepository.UpdateIsAccessTokenValid(pwShop.PwShopId, true);
                _logger.Info($"Updated Shop - UserId: {pwShop.ShopOwnerUserId}, " +
                            $"CurrencyId: {pwShop.CurrencyId}, " +
                            $"TimeZone: {pwShop.TimeZone} - and set IsAccessTokenValid = true");
            }
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
                Response.SuppressFormsAuthenticationRedirect = true;
                Response.TrySkipIisCustomErrors = true;

                return View(new AuthorizationProblemModel(returnUrl));
            }
        }

        // GET: /ShopifyAuth/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure(string returnUrl)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;

            return View(new AuthorizationProblemModel(returnUrl));
        }

        // GET: /ShopifyAuth/AccessTokenRefresh
        [AllowAnonymous]
        public ActionResult AccessTokenRefresh(string returnUrl)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;

            return View(new AuthorizationProblemModel(returnUrl));
        }

        // GET: /ShopifyAuth/AuthorizationFailure
        [AllowAnonymous]
        public ActionResult SevereAuthorizationFailure(string returnUrl)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;
            return View(new AuthorizationProblemModel(returnUrl));
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Dashboard", "Report");
        }

    }
}

