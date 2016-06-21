using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Web
{
    public class DefaultSecurityDataConfig
    {
        public static void Execute(IContainer container)
        {
            // Evil service locator, anti-pattern. But: it's ok enough, as this is close to composition root.
            var userManager = container.Resolve<ApplicationUserManager>();
            var roleManager = container.Resolve<ApplicationRoleManager>();

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(SecurityConfig.AdminRole))
            {
                roleManager.Create(new IdentityRole(SecurityConfig.AdminRole));
            }
            if (!roleManager.RoleExists(SecurityConfig.UserRole))
            {
                roleManager.Create(new IdentityRole(SecurityConfig.UserRole));
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