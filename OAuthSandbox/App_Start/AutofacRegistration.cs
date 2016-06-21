using System.Configuration;
using Autofac;
using MySql.Data.MySqlClient;
using ProfitWise.Web.Controllers;
using Push.Foundation.Web.Helpers;
using Push.Utilities.CastleProxies;
using Push.Utilities.Errors;
using Push.Utilities.Logging;

namespace ProfitWise.Web
{
    public class AutofacRegistration
    {
        public static IContainer Build()
        {            
            var builder = new ContainerBuilder();

            LoggerSingleton.Get = NLoggerImpl.RegistrationFactory("ProfitWise.Web", ActivityId.MessageFormatter);
            builder.Register(c => LoggerSingleton.Get()).As<ILogger>();

            ProfitWise.Data.AutofacRegistration.Build(builder);
            Push.Shopify.AutofacRegistration.Build(builder);

            // Database connection string...
            var mysqlConnectionString = 
                ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // ... and register configuration
            builder.Register<MySqlConnection>(ctx =>
            {
                var connectionstring = mysqlConnectionString;
                var connection = new MySqlConnection(connectionstring);
                connection.Open();
                return connection;
            });

            // TODO => register DbConnectionString...?

            // Push.Foundation.Web Identity Stuff
            var encryption_key = ConfigurationManager.AppSettings["security_aes_key"];
            var encryption_iv = ConfigurationManager.AppSettings["security_aes_iv"];
            Push.Foundation.Web.AutofacRegistration.Build(builder, encryption_key, encryption_iv);

            // Error forensics interceptors
            builder.RegisterType<ErrorForensics>();
            var registry = new InceptorRegistry();
            registry.Add(typeof(ErrorForensics));

            // Controller registration
            builder.RegisterType<UserHomeController>()
                .InstancePerRequest()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<UserMainController>()
                .InstancePerRequest()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<AdminHomeController>()
                .InstancePerRequest()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<AdminAuthController>()
                .InstancePerRequest()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ShopifyAuthController>()
                .InstancePerRequest()
                .EnableClassInterceptorsWithRegistry(registry);


            // *** Filter Registration
            //filters.Add(new HandleErrorAttributeImpl());
            //filters.Add(new IdentityCachingAttribute());

            //builder.Register(c => new HandleErrorAttributeImpl())
            //        .AsActionFilterFor<UserHomeController>(c => c.Index())
            //        .InstancePerHttpRequest();


            var container = builder.Build();
            return container;
        }
    }
}

