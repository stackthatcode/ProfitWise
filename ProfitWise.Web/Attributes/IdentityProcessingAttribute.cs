using System;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;

namespace ProfitWise.Web.Attributes
{
    public class IdentityProcessingAttribute : ActionFilterAttribute, IActionFilter
    {        
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var roleManager = DependencyResolver.Current.GetService<ApplicationRoleManager>();
            var dbContext = DependencyResolver.Current.GetService<ApplicationDbContext>();
            var credentialService = DependencyResolver.Current.GetService<IShopifyCredentialService>();
            var shopRepository = DependencyResolver.Current.GetService<PwShopRepository>();
            var signInManager = DependencyResolver.Current.GetService<ApplicationSignInManager>();
            var logger = DependencyResolver.Current.GetService<IPushLogger>();

            // Pull the User ID from OWIN plumbing...
            var userId = filterContext.HttpContext.User.ExtractUserId();
            var currentUrl = filterContext.HttpContext.Request.Url.PathAndQuery;

            if (userId == null)
            {
                logger.Debug($"Unable to successfully ExtractUserId - aborting ProcessIdentity");
                
                // It appears that the Authorize Attribute (which invokes Regenerate Identity) has failed
                filterContext.Result = AuthConfig.UnauthorizedAccessRedirect(currentUrl);
                return;
            }

            var user = dbContext.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                logger.Error($"Unable to retrieve User with Id {userId} - complete Authorization Failure");
                AuthConfig.GlobalSignOut(signInManager);
                filterContext.Result = AuthConfig.AuthorizationFailureRedirect(currentUrl);
                return;
            }

            var pwShop = shopRepository.RetrieveByUserId(userId);
            if (!pwShop.ValidAccessToken)
            {
                logger.Info($"The Access Token for PwShop {pwShop.PwShopId} requires a Refresh");
                AuthConfig.GlobalSignOut(signInManager);
                filterContext.Result = AuthConfig.AccessTokenRefresh(currentUrl);
                return;
            }

            // Retrieve User Access Roles
            var userRoleIds = user.Roles.Select(x => x.RoleId);
            var userRoles =
                roleManager.Roles
                    .Where(x => userRoleIds.Contains(x.Id))
                    .Select(x => x.Name)
                    .ToList();

            var result = credentialService.Retrieve(userId);
            if (result.Success == false)
            {
                // On failure of credential service, throw an error, which will redirect to Server Fault page
                logger.Error($"Unable to Retrieve Claims for User {userId} - " + result.Message);
                AuthConfig.GlobalSignOut(signInManager);
                filterContext.Result = AuthConfig.AuthorizationFailureRedirect(currentUrl);
                return;
            }

            var identity = new IdentitySnapshot()
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = userRoles,
                Email = user.Email,

                Impersonated = result.Impersonated,
                PwShop = pwShop,
                ShopDomain = result.ShopDomain,
            };

            logger.Debug($"Successfully hydrated User {user.Id} Identity Snapshot into HttpContext");
            filterContext.HttpContext.PushIdentitySnapshot(identity);
        }
    }
}
