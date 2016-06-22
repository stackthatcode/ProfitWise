using System;
using System.Linq;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Push.Foundation.Web.Identity;
using Push.Utilities.General;

namespace ProfitWise.Web
{
    public class DefaultSecurityDataConfig
    {
        public static void Execute(ILifetimeScope container)
        {
            // Evil service locator, anti-pattern. But: it's ok enough, as this is close to composition root.
            var userManager = container.Resolve<ApplicationUserManager>();
            var roleManager = container.Resolve<ApplicationRoleManager>();

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(SecurityConfig.AdminRole))
            {
                var result = roleManager.Create(new IdentityRole(SecurityConfig.AdminRole));
                if (result.Succeeded == false)
                {
                    throw new Exception($"RoleManager.Create (Admin) failed: {result.Errors.JoinByNewline()}");
                }
            }

            if (!roleManager.RoleExists(SecurityConfig.UserRole))
            {
                var result = roleManager.Create(new IdentityRole(SecurityConfig.UserRole));
                if (result.Succeeded == false)
                {
                    throw new Exception($"RoleManager.Create (User) failed: {result.Errors.JoinByNewline()}");
                }
            }

            var adminUser = userManager.FindByName(SecurityConfig.DefaultAdminEmail);
            if (adminUser == null)
            {
                var newAdminUser = new ApplicationUser()
                {
                    UserName = SecurityConfig.DefaultAdminEmail,
                    Email = SecurityConfig.DefaultAdminEmail,
                };

                var result = userManager.Create(newAdminUser, SecurityConfig.DefaultAdminPassword);
                if (result.Succeeded == false)
                {
                    throw new Exception($"UserManager.Create failed: {result.Errors.JoinByNewline()}");
                }

                var resultAddToAdmin = userManager.AddToRole(newAdminUser.Id, SecurityConfig.AdminRole);
                if (resultAddToAdmin.Succeeded == false)
                {
                    throw new Exception($"UserManager.AddToRole (Admin) failed: {resultAddToAdmin.Errors.JoinByNewline()}");
                }

                var resultAddToUser =userManager.AddToRole(newAdminUser.Id, SecurityConfig.UserRole);
                if (resultAddToUser.Succeeded == false)
                {
                    throw new Exception($"UserManager.AddToRole (User) failed: {resultAddToUser.Errors.JoinByNewline()}");
                }
            }
        }
    }
}
