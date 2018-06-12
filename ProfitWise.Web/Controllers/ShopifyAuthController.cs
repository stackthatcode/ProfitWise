﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Castle.Core.Internal;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using ProfitWise.Data.Configuration;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using ProfitWise.Data.Utility;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Helpers;
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
        private readonly CurrencyService _currencyService;

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
                ApiRepositoryFactory factory,
                CurrencyService currencyService)
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
            _currencyService = currencyService;
            _shopOrchestrationService = shopSynchronizationService;
        }


        // Shopify OAuth authentication & authorization flow
        [AllowAnonymous]
        public ActionResult Login(string shop, string returnUrl)
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

            if (ProfitWiseConfiguration.Settings.ShopifyApiKey.IsNullOrEmpty())
            {
                throw new Exception("Null or empty ShopifyApiKey - please check configuration");
            }

            var authUrl = 
                ShopifyAuthorizationService.BuildAuthorizationUrl(
                    scopes, shopUrl, ProfitWiseConfiguration.Settings.ShopifyApiKey, redirectUrl);

            return Redirect(authUrl.ToString());
        }
        
        [AllowAnonymous]
        public async Task<ActionResult> ShopifyReturn(string code, string shop, string returnUrl)
        {
            // Attempt to complete Shopify Authentication
            var profitWiseSignIn = await CompleteShopifyAuth(code, shop);
            if (profitWiseSignIn == null)
            {
                return GlobalConfig.Redirect(AuthConfig.ExternalLoginFailureUrl, returnUrl);
            }

            // Redirect to error page for invalid currency
            if (!_currencyService.CurrencyExists(profitWiseSignIn.Shop.Currency))
            {
                return GlobalConfig.Redirect(AuthConfig.UnsupportedCurrency, returnUrl);
            }

            // Create User
            var user = await UpsertAspNetUserAcct(profitWiseSignIn);
            if (user == null)
            {
                return GlobalConfig.Redirect(AuthConfig.ExternalLoginFailureUrl, returnUrl);
            }
            
            // Create/Update the Shop
            var result = UpsertProfitWiseShop(profitWiseSignIn, user);

            if (!result.Success)
            {
                if (result.Reason == ShopUpsertionFailureReason.ShopCurrencyChange)
                {
                    var queryString =
                        new QueryStringBuilder()
                            .Add("newCurrency", profitWiseSignIn.Shop.Currency)
                            .ToString();

                    var url = $"{AuthConfig.ShopCurrencyChangeUrl}?{queryString}";

                    return GlobalConfig.Redirect(url, returnUrl);
                }
                else
                {
                    return GlobalConfig.Redirect(AuthConfig.SevereAuthorizationFailureUrl, returnUrl);
                }
            }

            return UpsertBilling(user, returnUrl);
        }
        
        private async Task<ProfitWiseSignIn> CompleteShopifyAuth(string code, string shopDomain)
        {
            var apikey = ProfitWiseConfiguration.Settings.ShopifyApiKey;
            var apisecret = ProfitWiseConfiguration.Settings.ShopifyApiSecret;

            try
            {
                var nonAccessTokenCredentials =
                    ShopifyCredentials.Build(shopDomain, apikey, apisecret);

                var oauthRepository = _factory.MakeOAuthRepository(nonAccessTokenCredentials);

                var accessToken = oauthRepository.RetrieveAccessToken(code);

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
            // FYI, UserName == Shopify Shop Domain
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
                        await _signInManager.SignInAsync(newUser, isPersistent: true, rememberBrowser: true);

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

        private ShopUpsertionResult UpsertProfitWiseShop(ProfitWiseSignIn signIn, ApplicationUser user)
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
                    var newCurrencyId = _currencyService.AbbrToCurrencyId(signIn.Shop.Currency);

                    if (pwShop.CurrencyId != newCurrencyId)
                    {
                        return ShopUpsertionResult.Fail(ShopUpsertionFailureReason.ShopCurrencyChange);
                    }

                    _shopOrchestrationService
                        .UpdateShopAndAccessTokenValid(
                            user.Id, signIn.Shop.Currency, signIn.Shop.TimeZoneIana);
                }

                // Ensure that the Uninstall Webhook is in place
                _shopOrchestrationService.UpsertUninstallWebhook(user.Id, credentials);
                return ShopUpsertionResult.Succeed();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return ShopUpsertionResult.Fail(ShopUpsertionFailureReason.Exception);
            }

        }

        private ActionResult UpsertBilling(ApplicationUser user, string returnUrl)
        {
            var charge = _shopOrchestrationService.SyncAndRetrieveCurrentCharge(user.Id);

            if (charge != null && charge.LastStatus == ChargeStatus.Pending)
            {
                _logger.Info($"User attempting to login with Pending Shopify Charge {charge.PwChargeId}");

                // Redirect for Shopify Charge approval
                return View("JavaScriptRedirect", 
                    JavaScriptRedirectModel.BuildForChargeConfirm(charge.ConfirmationUrl));
            }

            if (charge == null || 
                charge.LastStatus.SystemMustCreateNewCharge() ||
                charge.MustDestroyOnNextLogin)
            {
                // Create ProfitWise subscription and save
                var verifyUrl = GlobalConfig.BaseUrl + $"/ShopifyAuth/VerifyBilling";

                // Invoke the Shopify API and save to ProfitWise SQL
                var newCharge =
                    charge != null && charge.MustDestroyOnNextLogin
                        ? _shopOrchestrationService.CreateCharge(user.Id, verifyUrl, 0)
                        : _shopOrchestrationService.CreateCharge(user.Id, verifyUrl);

                // Redirect for Shopify Charge approval
                return View("JavaScriptRedirect",
                    JavaScriptRedirectModel.BuildForChargeConfirm(newCharge.ConfirmationUrl));
            }

            return RedirectToLocal(GlobalConfig.BaseUrl);
        }

        [HttpGet]
        public ActionResult VerifyBilling(string charge_id)
        {
            var chargeAccepted = 
                _shopOrchestrationService.VerifyChargeAndScheduleRefresh(charge_id);            

            if (chargeAccepted)
            {
                return Redirect("~");
            }
            else
            {
                AuthConfig.GlobalSignOut(_signInManager);
                return View("BillingDeclined");
            }
        }



        // Webhook recipient
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
                showLoginLink: true, showBrowserWarning: true);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult UnsupportedCurrency(string returnUrl)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;
            return View();
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

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ShopCurrencyChange(string newCurrency, string returnUrl)
        {
            var model = new ShopCurrencyChange
            {
                NewCurrencyAbbr = newCurrency,
                NewCurrencyName = _currencyService.CurrencyByAbbr(newCurrency).Name,
            };
            return View(model);
        }


        private ActionResult AuthorizationProblem(
                string returnUrl, 
                string title, 
                string message, 
                bool showLoginLink = false, 
                bool showBrowserWarning = false)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;
           
            var url = GlobalConfig.BaseUrl + $"/ShopifyAuth/Login";
            var shopParameterExists = false;
            
            if (returnUrl != null)
            {
                url += $"?returnUrl={HttpUtility.UrlEncode(returnUrl)}";

                var shop = returnUrl.ExtractQueryParameter("shop");
                if (!CollectionExtensions.IsNullOrEmpty(shop))
                {
                    url += $"&shop={shop}";
                    shopParameterExists = true;
                }
            }

            var model = 
                new AuthorizationProblemModel(url)
                {
                    Title = title,
                    Message = message,
                    ShowLoginLink = shopParameterExists && showLoginLink,
                    ShowBrowserWarning = showBrowserWarning
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

