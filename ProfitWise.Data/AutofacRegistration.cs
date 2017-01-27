﻿using Autofac;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.ExchangeRateApis;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Processes;
using ProfitWise.Data.ProcessSteps;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.CastleProxies;
using Push.Utilities.CastleProxies;

namespace ProfitWise.Data
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<ErrorForensics>();
            builder.RegisterType<ExecutionTime>();

            builder.RegisterType<MultitenantFactory>();

            builder.RegisterType<ShopifyOrderRepository>();
            builder.RegisterType<PwShopRepository>();
            builder.RegisterType<PwBatchStateRepository>();
            builder.RegisterType<PwProductRepository>();
            builder.RegisterType<PwVariantRepository>();
            builder.RegisterType<PwCogsEntryRepository>();
            builder.RegisterType<CurrencyRepository>();
            builder.RegisterType<PwPickListRepository>();
            builder.RegisterType<PwReportRepository>();
            builder.RegisterType<PwReportFilterRepository>();
            builder.RegisterType<PwReportQueryRepository>();
            builder.RegisterType<SystemStateRepository>();
            builder.RegisterType<PwCogsDataUpdateRepository>();
            
            var registry = new InceptorRegistry();
            registry.Add(typeof(ExecutionTime));

            builder.RegisterType<ShopRefreshService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<OrderRefreshStep>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ProductRefreshStep>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ProductCleanupStep>().EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<CatalogBuilderService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<CurrencyService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<ShopSynchronizationService>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<CogsService>().EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ShopRefreshProcess>().EnableClassInterceptorsWithRegistry(registry);
            builder.RegisterType<CurrencyProcess>().EnableClassInterceptorsWithRegistry(registry);

            builder.RegisterType<ShopRequired>();

            builder.RegisterType<FixerApiConfig>();
            builder.RegisterType<FixerApiRepository>();
            builder.RegisterType<FixerApiRequestFactory>();

            // Critical piece for all database infrastructure to work smoothly
            builder.RegisterType<ConnectionWrapper>().InstancePerLifetimeScope();

            // Chicago, by default!
            builder.Register(x => new TimeZoneTranslator(6, 0));
        }
    }
}
