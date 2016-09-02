using System;
using System.Configuration;
using Autofac;
using Hangfire;
using ProfitWise.Data.Processes;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap.ConfigureApp();
            using (var container = AutofacRegistration.Build())
            {
                InvokeRefreshServices(container);
                Console.ReadLine();
            }
        }


        private static void InvokeRefreshServices(IContainer container)
        {
            var logger = container.Resolve<IPushLogger>();
            try
            {
                // This is for simulation purposes - in the future, we'll load a list of Users from database
                var userId = "c2e5bb33-0d4c-49c4-8204-09246b23352c";
                logger.Info("Hello! - Executing InvokeRefreshServices" + Environment.NewLine);

                using (var scope = container.BeginLifetimeScope())
                {
                    var refreshProcess = scope.Resolve<RefreshProcess>();
                    refreshProcess.Execute(userId);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);

                // Add notification service call here
                logger.Fatal(e.Message);
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
