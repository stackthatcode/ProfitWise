using System;
using Autofac;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using ProfitWise.Data.HangFire;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Batch
{
    public class Bootstrapper
    {
        public static IContainer ConfigureApp(bool runningHangFire = false)
        {
            var container = AutofacRegistration.Build(runningHangFire);

            // Connects the Autofac Container directly into HangFire
            GlobalConfiguration.Configuration.UseAutofacActivator(container);

            // HangFireLogProvider 
            LogProvider.SetCurrentLogProvider(new HangFireLogProvider());
            var options = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(1),
                PrepareSchemaIfNecessary = false,
            };
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection", options);

            return container;
        }


        // Preload Currency Cache to avoid contention
        //using (var scope = container.BeginLifetimeScope())
        //{
        //    var service = container.Resolve<CurrencyService>();
        //    service.LoadExchangeRateCache();
        //}

    }
}
