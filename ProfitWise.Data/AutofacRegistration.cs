using Autofac;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.ExchangeRateApis;
using ProfitWise.Data.Factories;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Processes;
using ProfitWise.Data.ProcessSteps;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Repositories.Multitenant;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using ProfitWise.Data.Utility;
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
            builder.RegisterType<ShopRepository>();
            builder.RegisterType<BatchStateRepository>();
            builder.RegisterType<ProductRepository>();
            builder.RegisterType<VariantRepository>();
            builder.RegisterType<CogsEntryRepository>();
            builder.RegisterType<ExchangeRateRepository>();
            builder.RegisterType<PickListRepository>();
            builder.RegisterType<ReportRepository>();
            builder.RegisterType<ReportFilterRepository>();
            builder.RegisterType<ProfitRepository>();
            builder.RegisterType<SystemRepository>();
            builder.RegisterType<CogsDownstreamRepository>();
            builder.RegisterType<GoodsOnHandRepository>();
            builder.RegisterType<AdminRepository>();
            builder.RegisterType<BillingRepository>();

            var registry = new InceptorRegistry();
            registry.Add(typeof(ExecutionTime));
            
            builder.RegisterType<OrderRefreshStep>();
            builder.RegisterType<ProductRefreshStep>();
            builder.RegisterType<ProductCleanupStep>();

            builder.RegisterType<ShopRefreshService>();
            builder.RegisterType<CatalogBuilderService>();
            builder.RegisterType<CatalogRetrievalService>();
            builder.RegisterType<CurrencyService>();
            builder.RegisterType<ShopOrchestrationService>();
            builder.RegisterType<ConsolidationService>();
            builder.RegisterType<CogsService>();
            builder.RegisterType<HangFireService>();
            builder.RegisterType<DataService>();
            
            builder.RegisterType<ShopRefreshProcess>();
            builder.RegisterType<ExchangeRateRefreshProcess>();
            builder.RegisterType<SystemCleanupProcess>();

            builder.RegisterType<MultitenantMethodLock>();

            builder.RegisterType<ShopRequired>();

            builder.RegisterType<FixerApiConfig>();
            builder.RegisterType<FixerApiRepository>();
            builder.RegisterType<FixerApiRequestFactory>();
        }
    }
}
