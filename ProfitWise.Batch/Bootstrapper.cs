using System;
using Autofac;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Batch
{
    public class Bootstrapper
    {
        public static IContainer ConfigureApp()
        {
            var container = AutofacRegistration.Build();

            // Logger wiring
            LoggerSingleton.Get =
                NLoggerImpl.LoggerFactory(
                    "ProfitWise.Batch", Formatters.TypeAndMethodNameFormatFactory());

            // HangFire wiring
            GlobalConfiguration.Configuration.UseAutofacActivator(container);
            LogProvider.SetCurrentLogProvider(new HangFireLogProvider());
            var options = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(1),
                PrepareSchemaIfNecessary = false,
            };
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection", options);

            // Preload Currency Cache to avoid contention
            //using (var scope = container.BeginLifetimeScope())
            //{
            //    var service = container.Resolve<CurrencyService>();
            //    service.LoadExchangeRateCache();
            //}

            return container;
        }
    }
}
