using System;
using System.Linq;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Logging;
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
            var logger = container.Resolve<IPushLogger>();


            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(SecurityConfig.AdminRole))
            {
                logger.Info($"Role {SecurityConfig.AdminRole} does not exist - adding to Roles");
                var result = roleManager.Create(new IdentityRole(SecurityConfig.AdminRole));
                if (result.Succeeded == false)
                {
                    throw new Exception($"RoleManager.Create (Admin) failed: {StringExtensions.JoinByNewline(result.Errors)}");
                }
            }

            if (!roleManager.RoleExists(SecurityConfig.UserRole))
            {
                logger.Info($"Role {SecurityConfig.UserRole} does not exist - adding to Roles");
                var result = roleManager.Create(new IdentityRole(SecurityConfig.UserRole));
                if (result.Succeeded == false)
                {
                    throw new Exception($"RoleManager.Create (User) failed: {StringExtensions.JoinByNewline(result.Errors)}");
                }
            }

            var adminUser = userManager.FindByName(SecurityConfig.DefaultAdminEmail);
            if (adminUser == null)
            {
                logger.Info($"Unable to locate default Sys Admin: {SecurityConfig.DefaultAdminEmail} - creating new Sys Admin");

                var newAdminUser = new ApplicationUser()
                {
                    UserName = SecurityConfig.DefaultAdminEmail,
                    Email = SecurityConfig.DefaultAdminEmail,
                };

                var result = userManager.Create(newAdminUser, SecurityConfig.DefaultAdminPassword);
                if (result.Succeeded == false)
                {
                    throw new Exception($"UserManager.Create failed: {StringExtensions.JoinByNewline(result.Errors)}");
                }

                var resultAddToAdmin = userManager.AddToRole(newAdminUser.Id, SecurityConfig.AdminRole);
                if (resultAddToAdmin.Succeeded == false)
                {
                    throw new Exception($"UserManager.AddToRole (Admin) failed: {StringExtensions.JoinByNewline(resultAddToAdmin.Errors)}");
                }

                var resultAddToUser =userManager.AddToRole(newAdminUser.Id, SecurityConfig.UserRole);
                if (resultAddToUser.Succeeded == false)
                {
                    throw new Exception($"UserManager.AddToRole (User) failed: {StringExtensions.JoinByNewline(resultAddToUser.Errors)}");
                }
            }
        }
    }
}
