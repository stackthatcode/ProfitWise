using System;
using System.Configuration;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Providers.Shopify;
using Push.Utilities.Helpers;
using Push.Utilities.Security;
using Push.Utilities.Web.Identity;

namespace ProfitWise.Web
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            // Credential Service
            var encryption_key = ConfigurationManager.AppSettings["security_aes_key"];
            var encryption_iv = ConfigurationManager.AppSettings["security_aes_iv"];
            ShopifyCredentialService.EncryptionService = new EncryptionService(encryption_key, encryption_iv);


            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Customized Role Manager
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);


            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/ShopifyAuth/Index"),
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


            // Temporary, before we create a custom configuration section that can be encrypted via machine key
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
            app.UseShopifyAuthentication(shopify_options);
        }


        public void ConfigureDefaultSecurityData()
        {
            // TODO: why does this work? What about the configuration operations? Hmmm...
            var context = ApplicationDbContext.Create();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            IdentityResult roleResult;


            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(SecurityConfig.AdminRole))
            {
                roleResult = roleManager.Create(new IdentityRole(SecurityConfig.AdminRole));
            }
            if (!roleManager.RoleExists(SecurityConfig.UserRole))
            {
                roleResult = roleManager.Create(new IdentityRole(SecurityConfig.UserRole));
            }

            var adminUser = userManager.FindByName(SecurityConfig.DefaultAdminEmail);
            if (adminUser == null)
            {
                var newAdminUser = new ApplicationUser()
                {
                    UserName = SecurityConfig.DefaultAdminEmail,
                    Email = SecurityConfig.DefaultAdminEmail,
                };

                userManager.Create(newAdminUser, SecurityConfig.DefaultAdminPassword);
                userManager.AddToRole(newAdminUser.Id, SecurityConfig.AdminRole);
                userManager.AddToRole(newAdminUser.Id, SecurityConfig.UserRole);
            }
        }
    }
}

