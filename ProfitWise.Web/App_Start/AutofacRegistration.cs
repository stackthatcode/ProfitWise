using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using ProfitWise.Data.Database;
using ProfitWise.Web.Controllers;
using Push.Foundation.Utilities.CastleProxies;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Utilities.CastleProxies;


namespace ProfitWise.Web
{
    public class AutofacRegistration
    {
        public static IContainer Build()
        {            
            var builder = new ContainerBuilder();

            // Logging
            LoggerSingleton.Get = 
                NLoggerImpl.LoggerFactory("ProfitWise.Web", ActivityId.MessageFormatter);
            builder.Register(c => LoggerSingleton.Get()).As<IPushLogger>();


            // Push.Foundation.Web Identity Stuff
            var encryption_key = ConfigurationManager.AppSettings["security_aes_key"];
            var encryption_iv = ConfigurationManager.AppSettings["security_aes_iv"];
            Push.Foundation.Web.AutofacRegistration.Build(builder, encryption_key, encryption_iv);


            // ProfitWise.Data API registration
            ProfitWise.Data.AutofacRegistration.Build(builder);

            // Push.Shopify API registration
            Push.Shopify.AutofacRegistration.Build(builder);


            // Database connection string...
            var connectionString = 
                ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // ... and register configuration
            builder
                .Register(ctx =>
                {
                    var connectionstring = connectionString;
                    var connection = new SqlConnection(connectionstring);
                    connection.Open();
                    return connection;
                })
                .As<SqlConnection>()
                .As<DbConnection>()
                .As<IDbConnection>()
                .InstancePerLifetimeScope();

            // Critical piece for all database infrastructure to work smoothly
            builder.RegisterType<ConnectionWrapper>().InstancePerLifetimeScope();
            
            // Error forensics interceptors
            builder.RegisterType<ErrorForensics>();
            
            // Controller registration   
            builder.RegisterType<ErrorController>();
            builder.RegisterType<ReportController>();
            builder.RegisterType<AdminHomeController>();
            builder.RegisterType<AdminAuthController>();
            builder.RegisterType<ShopifyAuthController>();
            builder.RegisterType<CogsServiceController>();
            builder.RegisterType<CogsController>();
            builder.RegisterType<FilterServiceController>();
            builder.RegisterType<QueryServiceController>();
            builder.RegisterType<ReportServiceController>();
            builder.RegisterType<PreferencesController>();
            builder.RegisterType<ContentController>();
            builder.RegisterType<ConsolServiceController>();
                
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }
    }
}

