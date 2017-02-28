using System.Web.Mvc;
using Hangfire.Dashboard;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Web.Attributes
{
    public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {            
            // In case you need an OWIN context, use the next line,
            // `OwinContext` class is the part of the `Microsoft.Owin` package.
            var owincontext = new OwinContext(context.GetOwinEnvironment());

            // Only allow Admins to see HangFire Dashboard
            var userId = owincontext.Authentication.User.Identity.GetUserId();
            if (userId == null)
            {
                return false;
            }

            var userManager = DependencyResolver.Current.GetService<ApplicationUserManager>();
            var roles = userManager.GetRoles(userId);
            return roles.Contains(SecurityConfig.AdminRole);
        }
    }
}