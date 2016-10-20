using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Batch
{
    class _HangFireStuff
    {
        // *** SAVE FOR NOW *** //
        private static void HangFireStuff()
        {
            LoggerSingleton.Get = NLoggerImpl.RegistrationFactory("Initializer");
            var logger = LoggerSingleton.Get();

            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);

                // Wait for graceful server shutdown.
                var server = new BackgroundJobServer();

                // Logger
                logger.Info("PROCESS START:" + DateTime.Now + Environment.NewLine);

                //RecurringJob.AddOrUpdate(() => LoggerSingleton.Get().Info("Update Job Starting"), "");
                //var id = BackgroundJob.Enqueue(() => Console.WriteLine("Hello, "));
                //BackgroundJob.ContinueWith(id, () => Console.WriteLine("world!"));

                //new Hangfire.BackgroundJobServer().

                //Console.ReadLine();
                //server.Dispose();

                //throw new Exception("Wassup???");
                // DoStuff();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Console.ReadLine();
            }
            finally
            {
                logger.Info(Environment.NewLine + "PROCESS FINISH:" + DateTime.Now + Environment.NewLine);
            }
        }
    }
}
