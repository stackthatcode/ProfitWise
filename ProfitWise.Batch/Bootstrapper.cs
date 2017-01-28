using Autofac;
using Hangfire;
using Hangfire.Logging;
using ProfitWise.Data.HangFire;
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

            return container;
        }
    }
}
