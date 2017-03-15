using System;
using System.Linq;
using System.Web;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Push.Foundation.Web.Identity;


namespace ProfitWise.Web
{
    public enum AuthProblemCode
    {
        UnauthorizedAccess = 1,
        ExternalLoginFailure = 2,
        AccessTokenRefresh = 3,
        SevereAuthorizationFailure = 4,
        BillingProblem = 5,
        BillingIncomplete = 6,
    }

    public class AuthConfig
    {
        // For Convenient Access
        public static string LoginUrl(string domain, string returnUrl = null)
        {
            var url = "/ShopifyAuth/Login?shop=" + domain +
                (returnUrl != null ? $"&returnUrl={HttpUtility.UrlEncode(returnUrl)}" : "");
            return url;
        }

        public const string UnauthorizedAccessUrl = "/ShopifyAuth/UnauthorizedAccess";
        public const string ExternalLoginFailureUrl = "/ShopifyAuth/ExternalLoginFailure";
        public const string AccessTokenRefreshUrl = "/ShopifyAuth/AccessTokenRefresh";
        public const string SevereAuthorizationFailureUrl = "/ShopifyAuth/SevereAuthorizationFailure";
        public const string BillingProblemUrl = "/ShopifyAuth/BillingProblem";
        public const string BillingIncomplete = "/ShopifyAuth/BillingIncomplete";


        public static void Configure(IAppBuilder app, IContainer autofacContainer)
        {
            var authorizationProblemUrls = new[]
            {
                AuthConfig.UnauthorizedAccessUrl,
                AuthConfig.SevereAuthorizationFailureUrl,
                AuthConfig.AccessTokenRefreshUrl,
                AuthConfig.ExternalLoginFailureUrl,
                AuthConfig.BillingProblemUrl,
                AuthConfig.BillingIncomplete,
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
        }

        public static void GlobalSignOut(ApplicationSignInManager signInManager)
        {
            signInManager.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            signInManager.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

    }
}

