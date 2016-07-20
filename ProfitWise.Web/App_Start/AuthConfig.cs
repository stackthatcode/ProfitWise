using System;
using System.Configuration;
using System.Security.Claims;
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
                LoginPath = new PathString("/ShopifyAuth/Refresh"),
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
                        // TODO: simulate exception handling from this

                        // Retrieve the OAuth access token to store for subsequent API calls
                        string accessToken = context.AccessToken;

                        // Shopify shop Id.
                        string shopId = context.Id;

                        // Shopify shop name
                        string shopName = context.ShopName;

                        // Shopify domain
                        string domain = context.ShopifyDomain;

                        // Retrieve the Shopify shop's primary email address
                        string shopPrimaryEmailAddress = context.Email;

                        // You can even retrieve the full JSON-serialized shop
                        var serializedShopInformation = context.Shop;
                        
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
    }
}

