﻿using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using MySql.Data.MySqlClient;
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
                NLoggerImpl.RegistrationFactory("ProfitWise.Web", ActivityId.MessageFormatter);
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
            var mysqlConnectionString = 
                ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            // ... and register configuration
            builder
                .Register<MySqlConnection>(ctx =>
                {
                    var connectionstring = mysqlConnectionString;
                    var connection = new MySqlConnection(connectionstring);
                    connection.Open();
                    return connection;
                })
                .AsSelf()
                .As<DbConnection>()
                .As<IDbConnection>();


            // Error forensics interceptors
            builder.RegisterType<ErrorForensics>();
            var registry = new InceptorRegistry();
            //registry.Add(typeof(ErrorForensics));

            // Controller registration            
            builder.RegisterType<UserMainController>()
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


            // *** Filter Registration
            //filters.Add(new HandleErrorAttributeImpl());
            //filters.Add(new IdentityCachingAttribute());

            //builder.Register(c => new HandleErrorAttributeImpl())
            //        .AsActionFilterFor<UserHomeController>(c => c.Index())
            //        .InstancePerHttpRequest();


            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }
    }
}

