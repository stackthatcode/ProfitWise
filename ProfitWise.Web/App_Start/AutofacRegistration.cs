﻿using System.Configuration;
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
            var registry = new InceptorRegistry();
            //registry.Add(typeof(ErrorForensics));

            // Controller registration   
            builder.RegisterType<ErrorController>()
                .InstancePerDependency()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ReportController>()
                .InstancePerDependency()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<AdminHomeController>()
                .InstancePerDependency()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<AdminAuthController>()
                .InstancePerDependency()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ShopifyAuthController>()
                .InstancePerDependency()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<CogsServiceController>()
                .InstancePerDependency()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<CogsController>()
                .InstancePerDependency()
                .EnableClassInterceptorsWithRegistry(registry);            
            builder.RegisterType<FilterServiceController>()
                .InstancePerDependency()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<QueryServiceController>()
                .InstancePerDependency()
                .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ReportServiceController>()
                 .InstancePerDependency()
                 .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<PreferencesController>()
                 .InstancePerDependency()
                 .EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ContentController>()
                 .InstancePerDependency()
                 .EnableClassInterceptorsWithRegistry(registry);
            

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }
    }
}

