using Autofac;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.ExchangeRateApis;
using ProfitWise.Data.Factories;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Processes;
using ProfitWise.Data.ProcessSteps;
using ProfitWise.Data.Repositories.Multitenant;
using ProfitWise.Data.Repositories.System;
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
            builder.RegisterType<TourRepository>();
            builder.RegisterType<UploadRepository>();

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
            builder.RegisterType<TourService>();
            builder.RegisterType<CalendarPopulationService>();
            builder.RegisterType<FileLocator>();

            builder.RegisterType<ShopRefreshProcess>();
            builder.RegisterType<ExchangeRateRefreshProcess>();
            builder.RegisterType<SystemCleanupProcess>();
            
            builder.RegisterType<ShopRequired>();

            builder.RegisterType<ExchangeRateApiConfig>();

            builder.RegisterType<FixerApiRepository>();
            builder.RegisterType<FixerApiRequestFactory>();

            builder.RegisterType<OerApiRepository>();
            builder.RegisterType<OerApiRequestFactory>();

            builder.RegisterType<TimeZoneTranslator>();
        }
    }
}
