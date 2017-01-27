using System.Data.Entity;
using System.Web;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Push.Foundation.Utilities.CastleProxies;
using Push.Foundation.Web.Http;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Web.Security;
using Push.Foundation.Web.Shopify;
using Push.Utilities.CastleProxies;

namespace Push.Foundation.Web
{
    public class AutofacRegistration
    {
        public static void Build(
            ContainerBuilder builder, string encryption_key, string encryption_iv)
        {
            // OWIN framework objects
            builder
                .RegisterType<ApplicationDbContext>()
                .As<DbContext>()
                .As<ApplicationDbContext>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ApplicationRoleManager>();
            builder.RegisterType<ApplicationUserManager>();
            builder.RegisterType<ApplicationSignInManager>();
            builder.RegisterType<ClaimsRepository>();
            builder.RegisterType<OwinUserService>();

            builder
                .RegisterType<UserStore<ApplicationUser>>()
                .As<IUserStore<ApplicationUser>>()
                .As<UserStore<ApplicationUser>>();

            builder
                .Register(ctx => new HttpContextWrapper(HttpContext.Current).GetOwinContext().Authentication)
                .As<IAuthenticationManager>();

            builder.RegisterType<RoleStore<IdentityRole>>();
            
            // We need to pull the DataProtectionProvider from the App Builder
            builder
                .RegisterType<MachineKeyProtectionProvider>()
                .As<IDataProtectionProvider>();
            builder
                .RegisterType<MachineKeyDataProtector>()
                .As<IDataProtector>();

            builder
                .RegisterType<DataProtectorTokenProvider<ApplicationUser>>();
            
            builder.Register(
                ctx => new EncryptionService(encryption_key, encryption_iv))
                .As<IEncryptionService>();

            // Our customer Push Services
            var registry = new InceptorRegistry();
            registry.Add(typeof(ExecutionTime));

            builder
                .RegisterType<EmailService>()
                .As<EmailService>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder
                .RegisterType<SmsService>()
                .As<SmsService>()
                .EnableClassInterceptorsWithRegistry(registry);

            builder
                .RegisterType<ShopifyCredentialService>()
                .As<IShopifyCredentialService>()
                .EnableClassInterceptorsWithRegistry(registry);


            // Http Client
            builder.RegisterType<HttpClientFacadeConfig>();
            builder.RegisterType<Http.HttpClient>().As<IHttpClient>();
            builder.RegisterType<HttpClientFacade>().As<IHttpClientFacade>();
        }
    }
}
