using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProfitWise.Data.Repositories;
using ProfitWise.Web.Controllers;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;

namespace ProfitWise.Web.Attributes
{
    public class IdentityCachingAttribute : ActionFilterAttribute, IActionFilter
    {        
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var roleManager = DependencyResolver.Current.GetService<ApplicationRoleManager>();
            var dbContext = DependencyResolver.Current.GetService<ApplicationDbContext>();
            var credentialService = DependencyResolver.Current.GetService<IShopifyCredentialService>();
            var shopRepository = DependencyResolver.Current.GetService<PwShopRepository>();
            var logger = DependencyResolver.Current.GetService<IPushLogger>();

            // Pull the User ID from OWIN plumbing...
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

                    var result = credentialService.Retrieve(userId);
                    if (result.Success == false)
                    {
                        throw new Exception(result.Message);
                    }

                    var shop = shopRepository.RetrieveByUserId(userId);

                    userBrief = new UserBrief()
                    {
                        Id = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                        Roles = userRoles,
                        Shop = shop,

                        Domain = result.ShopDomain,
                        ShopName = result.ShopDomain.ShopName(),
                    };

                    filterContext.HttpContext.PushUserBriefToContext(userBrief);
                }
                else
                {

                    var authenticationManager = filterContext.HttpContext.GetOwinContext().Authentication;
                    authenticationManager.SignOut();

                    var route = RouteValueDictionaryExtensions
                        .FromController<ShopifyAuthController>(x => x.UnauthorizedAccess(null));
                    filterContext.Result = new RedirectToRouteResult(route);
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
