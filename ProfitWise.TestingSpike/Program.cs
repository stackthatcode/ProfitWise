using System;
using System.Configuration;
using Hangfire;
using ProfitWise.Batch.Factory;
using ProfitWise.Batch.Products;
using Push.Shopify.Repositories;
using Push.Utilities.Logging;

namespace ProfitWise.Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap.ConfigureApp();
            InvokeProductRefreshService();
        }


        private static void InvokeProductRefreshService()
        {
            var logger = LoggerSingleton.Get();

            var userId = "a4ae6621-57ec-4ca8-837f-0b439e3cc710";
            var clientFactory = new ShopifyClientFactory();
            var productRefreshService = new ProductRefreshService(userId, logger);
            productRefreshService.Execute();
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
