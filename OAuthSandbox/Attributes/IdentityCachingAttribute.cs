using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OAuthSandbox.Controllers;
using OAuthSandbox.Models;
using ProfitWise.Web.Controllers;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;

namespace OAuthSandbox.Attributes
{
    public class IdentityCachingAttribute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var dbContext = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));

            var userId = filterContext.HttpContext.User.ExtractUserId();
            UserBrief userBrief = null;

            if (userId != null)
            {
                var user = dbContext.Users.FirstOrDefault(x => x.Id == userId);

                if (user != null)
                {
                    var userRoleIds = user.Roles.Select(x => x.RoleId);
                    var userRoles =
                        roleManager.Roles
                            .Where(x => userRoleIds.Contains(x.Id))
                            .Select(x => x.Name)
                            .ToList();

                    userBrief = new UserBrief()
                    {
                        Id = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                        Roles = userRoles,
                    };

                    filterContext.HttpContext.StoreUserBriefInContext(userBrief);
                }
                else
                {
                    var authenticationManager = filterContext.HttpContext.GetOwinContext().Authentication;
                    authenticationManager.SignOut();

                    var route = RouteValueDictionaryExtensions.FromController<ShopifyAuthController>(x => x.Index(null));
                    filterContext.Result = new RedirectToRouteResult(route);
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}