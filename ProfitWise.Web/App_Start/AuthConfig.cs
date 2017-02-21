using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Providers.Shopify;
using ProfitWise.Data.Configuration;
using Push.Foundation.Web.Identity;


namespace ProfitWise.Web
{
    public class AuthConfig
    {
        // For Convenient Access
        public const string UnauthorizedAccessUrl = "/ShopifyAuth/UnauthorizedAccess";
        public const string ExternalLoginFailureUrl = "/ShopifyAuth/ExternalLoginFailure";
        public const string AccessTokenRefreshUrl = "/ShopifyAuth/AccessTokenRefresh";
        public const string SevereAuthorizationFailureUrl = "/ShopifyAuth/SevereAuthorizationFailure";

        public static void Configure(IAppBuilder app, IContainer autofacContainer)
        {
            var authorizationProblemUrls = new[]
            {
                AuthConfig.UnauthorizedAccessUrl,
                AuthConfig.SevereAuthorizationFailureUrl,
                AuthConfig.AccessTokenRefreshUrl,
                AuthConfig.ExternalLoginFailureUrl
            };

            app.UseAutofacMiddleware(autofacContainer);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
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
                            regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager)),

                    // Note 01/13/2017 - this is a countermeasure to prevent OWIN from intercepting 401 responses
                    // ... and automatically sending uses to the LoginPath
                    OnApplyRedirect = ctx =>
                    {
                        if (!authorizationProblemUrls.Any(x => ctx.Request.Uri.PathAndQuery.Contains(x)))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    },
                }
            });

            // This enables the OAuth flow to create a temporary 
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Shopify Authorization
            var shopify_options = new ShopifyAuthenticationOptions()
            {
                ApiKey = "50d69dbaf54ee35929a946790d5884e4", // ProfitWiseConfiguration.Settings.ShopifyApiKey,
                ApiSecret = "eb52fb7d5612e383d2e9513001a012eb", // ProfitWiseConfiguration.Settings.ShopifyApiSecret,

                Provider = new ShopifyAuthenticationProvider
                {
                    OnAuthenticated = async context =>
                    {
                        string accessToken = context.AccessToken;
                        var domain = context.ShopifyDomain;
                        var serializedShopInformation = context.Shop.ToString();

                        // Currently unused Shop information
                        string shopPrimaryEmailAddress = context.Email;

                        // Add all this good stuff to these External Cookie Claims
                        context.Identity.AddClaim(
                            new Claim(SecurityConfig.ShopifyOAuthAccessTokenClaimExternal, accessToken));

                        context.Identity.AddClaim(
                            new Claim(SecurityConfig.ShopifyDomainClaimExternal, domain));

                        context.Identity.AddClaim(
                            new Claim(SecurityConfig.ShopifyShopSerializedExternal, serializedShopInformation));
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

    }
}

