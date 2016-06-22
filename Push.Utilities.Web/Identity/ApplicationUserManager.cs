using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Push.Foundation.Web.Identity
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(
                IUserStore<ApplicationUser> store,
                DataProtectorTokenProvider<ApplicationUser> userTokenProvider,
                EmailService emailService, 
                SmsService smsService)
            : base(store)
        {
            this.EmailService = emailService;
            this.SmsService = smsService;
            this.UserTokenProvider = userTokenProvider;
            ApplyDefaultSettings(this);
        }


        public static void ApplyDefaultSettings(ApplicationUserManager manager)
        {
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                //RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                //RequireNonLetterOrDigit = true,
                //RequireDigit = true,
                //RequireLowercase = true,
                //RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;            
        }
    }
}

