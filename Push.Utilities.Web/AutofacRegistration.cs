using System;
using System.Data.Entity;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Security;

namespace Push.Foundation.Web
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder, string encryption_key, string encryption_iv)
        {
            builder
                .RegisterType<ApplicationDbContext>()
                .As<DbContext>()
                .As<ApplicationDbContext>();

            builder.RegisterType<ApplicationRoleManager>();
            builder.RegisterType<ShopifyCredentialService>();
            builder.RegisterType<UserStore<ApplicationUser>>()
                .As<IUserStore<ApplicationUser>>()
                .As<UserStore<ApplicationUser>>();

            builder.RegisterType<ApplicationUserManager>();

            builder.Register<EncryptionService>(
                ctx => new EncryptionService(encryption_key, encryption_iv))
                .As<IEncryptionService>();

            builder.RegisterType<ShopifyCredentialService>();

            // TODO: how can we get around the sloppy OWIN stuff...?
            //builder.RegisterType<ApplicationSignInManager>();
        }
    }
}
