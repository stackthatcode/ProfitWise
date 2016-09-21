using System;
using System.Configuration;
using Autofac;
using Hangfire;
using ProfitWise.Data.Processes;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.TimeZone;


namespace ProfitWise.Batch
{
    class Program
    {
        static void Main(string[] args)
        {

            Bootstrap.ConfigureApp();
            using (var container = AutofacRegistration.Build())
            {
                RefreshServiceForSingleUser(container);
                Console.Write("Please hit enter...");
                Console.ReadLine();
            }
        }

        private static void TestTimeZoneTranslation(IContainer container)
        {
            var translator = container.Resolve<TimeZoneTranslator>();
            var result =
                translator.TranslateToTimeZone(
                    new DateTime(2016, 9, 9, 6, 30, 0),
                    "(GMT-06:00) Central Time (US & Canada)");
            var result2 =
                translator.TranslateToTimeZone(
                    new DateTime(2016, 9, 9, 6, 30, 0),
                    "(GMT-05:00) America/New_York");
        }


        private static void InvokeRefreshServices(IContainer container)
        {
            var logger = container.Resolve<IPushLogger>();
            try
            {
                logger.Info("Hello! - Executing InvokeRefreshServices" + Environment.NewLine);
                SimulateRefreshServiceFor1000Users(container);
            }
            catch (Exception e)
            {
                logger.Error(e);

                // Add notification service call here
                logger.Fatal(e.Message);
            }
        }

        private static void RefreshServiceForSingleUser(IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var userId = "c71e8244-e472-4f12-b55c-2c2f72500be0";

                //var userId = "c2e5bb33-0d4c-49c4-8204-09246b23352c";
                //var userId = "32c72971-c975-41dc-8487-12f372a2da53";
                var refreshProcess = scope.Resolve<RefreshProcess>();
                refreshProcess.Execute(userId);
            }
        }

        private static void SimulateRefreshServiceFor1000Users(IContainer container)
        {
            int artificialUserId = 1000;
            while (artificialUserId < 2000)
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var refreshProcess = scope.Resolve<RefreshProcess>();
                    refreshProcess.Execute(artificialUserId.ToString());
                }
                artificialUserId++;
            }
        }
        
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
