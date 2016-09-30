using System;
using System.Configuration;
using System.Security.Claims;
using System.Web.Mvc;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Providers.Shopify;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Web
{
    public class AuthConfig
    {
        // Could easily locate this in the ShopifyAuthController...
        public const string UnauthorizedAccessUrl = "/ShopifyAuth/UnauthorizedAccess";
        public const string AuthorizationFailureUrl = "/ShopifyAuth/AuthorizationFailure";
        public const string AccessTokenRefreshUrl = "/ShopifyAuth/AccessTokenRefresh";


        public static void Configure(IAppBuilder app, IContainer autofacContainer)
        {
            app.UseAutofacMiddleware(autofacContainer);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString(UnauthorizedAccessUrl),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = 
                        SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                            validateInterval: TimeSpan.FromMinutes(30),
                            regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

            // This enables the OAuth flow to create a temporary 
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Shopify Authorization
            var shopify_config_apikey = ConfigurationManager.AppSettings["shopify_config_apikey"];
            var shopify_config_apisecret = ConfigurationManager.AppSettings["shopify_config_apisecret"];            
            var shopify_options = new ShopifyAuthenticationOptions()
            {
                ApiKey = shopify_config_apikey,
                ApiSecret = shopify_config_apisecret,

                Provider = new ShopifyAuthenticationProvider
                {
                    OnAuthenticated = async context =>
                    {
                        // Retrieve the OAuth access token to store for subsequent API calls
                        string accessToken = context.AccessToken;

                        // Shop Id, Name, Domain and Email
                        string domain = context.ShopifyDomain;

                        // Currently unused Shop information
                        //string shopId = context.Id;
                        //string shopName = context.ShopName;
                        //string shopPrimaryEmailAddress = context.Email;
                        //var serializedShopInformation = context.Shop;

                        context.Identity.AddClaim(
                            new Claim(SecurityConfig.ShopifyOAuthAccessTokenClaimExternal, accessToken));

                        context.Identity.AddClaim(
                            new Claim(SecurityConfig.ShopifyDomainClaimExternal, domain));
                    },
                }
            };

            shopify_options.Scope.Add("read_orders");
            shopify_options.Scope.Add("read_products");
            app.UseShopifyAuthentication(shopify_options);
        }

        public static void GlobalSignOut(ApplicationSignInManager signInManager)
        {
            signInManager.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            signInManager.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }


        public static RedirectResult UnauthorizedAccessRedirect(string redirectUrl)
        {
            var url = $"{AuthConfig.UnauthorizedAccessUrl}?returnUrl={redirectUrl}";
            return new RedirectResult(url);
        }

        public static RedirectResult AuthorizationFailureRedirect(string redirectUrl)
        {
            var url = $"{AuthConfig.AuthorizationFailureUrl}?returnUrl={redirectUrl}";
            return new RedirectResult(url);
        }

        public static RedirectResult AccessTokenRefresh(string redirectUrl)
        {
            var url = $"{AuthConfig.AccessTokenRefreshUrl}?returnUrl={redirectUrl}";
            return new RedirectResult(url);
        }


    }
}

