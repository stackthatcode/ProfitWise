﻿using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using ProfitWise.Data.Configuration;
using ProfitWise.Data.Database;
using ProfitWise.Data.Services;
using ProfitWise.Web.Controllers;
using Push.Foundation.Utilities.CastleProxies;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Shopify.HttpClient;


namespace ProfitWise.Web
{
    public class AutofacRegistration
    {
        public static IContainer Build()
        {            
            var builder = new ContainerBuilder();

            // Push.Foundation.Web relies on consumers to supply Key and IV for its Encryption Service
            Push.Foundation.Web.AutofacRegistration.Build(builder, 
                    ProfitWiseConfiguration.Settings.ClaimKey, 
                    ProfitWiseConfiguration.Settings.ClaimIv,
                    ProfitWiseConfiguration.Settings.ShopifyApiSecret);

            // ProfitWise.Data API registration
            Data.AutofacRegistration.Build(builder);


            // Inject configuration after everything else to make sure it overwrites prior registrations

            // Logging - configure to use HTTP context-stored Activity ID
            Func<string, string> activityIdFormatter = x => ActivityId.Current + "|" + x;

            // The Singleton should only be used by the Application Start and End events
            // *** FAIL 
            LoggerSingleton.Get = NLoggerImpl.LoggerFactory("ProfitWise.Web", activityIdFormatter);

            builder.Register(c => LoggerSingleton.Get()).As<IPushLogger>();

            // Push.Shopify API registration
            Push.Shopify.AutofacRegistration.Build(builder);
            builder.Register(x => new ShopifyClientConfig()
            {
                RetryLimit = ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyRetryLimit", 3),
                Timeout = ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyHttpTimeout", 60000),
                ThrottlingDelay = ConfigurationManager.AppSettings.GetAndTryParseAsInt("ShopifyThrottlingDelay", 500),
                RetriesEnabled = ConfigurationManager.AppSettings.GetAndTryParseAsBool("ShopifyRetriesEnabled", true)
            });

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
            builder.RegisterType<ProfitServiceController>();
            builder.RegisterType<GoodsOnHandServiceController>();            
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

