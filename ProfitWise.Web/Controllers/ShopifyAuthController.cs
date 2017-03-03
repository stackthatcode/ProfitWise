﻿using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Model.Shop;
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

            if (externalLoginCookieResult == SignInStatus.Success)
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

            PwShop pwShop;
            using (var transaction = _shopSynchronizationService.InitiateTransaction())
            {
                var shopJson = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyShopSerializedExternal);
                var shop = Shop.MakeFromJson(shopJson.Value);
                pwShop = _shopSynchronizationService.UpsertShop(user.Id, shop);
                transaction.Commit();
            }
            return pwShop;
        }
        
        private ActionResult UpsertBilling(ApplicationUser user, string returnUrl)
        {
            var shop = _shopRepository.RetrieveByUserId(user.Id);
            if (shop.ShopifyRecurringChargeId == null)
            {
                // Create ProfitWise subscription and save
                var verifyUrl = GlobalConfig.BaseUrl + "/ShopifyAuth/VerifyBilling";
                var charge = _billingService.CreateCharge(user.Id, verifyUrl);

                // Redirect for Shopify Charge approval
                return View("ChargeConfirm", 
                    new ChargeConfirmModel() { ConfirmationUrl = charge.confirmation_url });
            }

            if (!shop.IsBillingValid)
            {
                // Redirect for Shopify Charge approval
                return View("ChargeConfirm", 
                    new ChargeConfirmModel() { ConfirmationUrl = shop.ConfirmationUrl });
            }
            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        public ActionResult VerifyBilling(string charge_id)
        {
            var userId = HttpContext.User.ExtractUserId();
            var charge = _billingService.RetrieveCharge(userId);
            var shop = _shopRepository.RetrieveByUserId(userId);
            var status = charge.status.ToChargeStatus();

            if (status == ChargeStatus.Accepted || status == ChargeStatus.Active)
            {                
                _shopRepository.UpdateIsBillingValid(shop.PwShopId, true);
                _hangFireService.TriggerInitialShopRefresh(userId);
                return Redirect("~");
            }
            else
            {
                _shopRepository.UpdateRecurringCharge(shop.PwShopId, null, null);
                return BillingDeclined();
            }
        }

        [HttpGet]
        public ActionResult BillingDeclined()
        {
            AuthConfig.GlobalSignOut(_signInManager);
            return View("BillingDeclined");
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


