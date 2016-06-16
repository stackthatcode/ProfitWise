using System;
using System.Configuration;
using Hangfire;
using Microsoft.AspNet.Identity.EntityFramework;
using ProfitWise.Batch.Factory;
using ProfitWise.Batch.Orders;
using ProfitWise.Batch.Products;
using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;
using Push.Utilities.Logging;
using Push.Utilities.Web.Identity;

namespace ProfitWise.Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap.ConfigureApp();
            InvokeRefreshServices();
        }


        private static void InvokeRefreshServices()
        {
            var logger = LoggerSingleton.Get();
            try
            {

                var userId = "a4ae6621-57ec-4ca8-837f-0b439e3cc710";
                //var productRefreshService = new ProductRefreshService(userId, logger);
                //productRefreshService.Execute();

                var shopifyClientFactory = new ShopifyHttpClientFactory(logger);
                var shopifyNaughtyClientFactory = new ShopifyNaughtyClientFactory(logger);
                var shopifyHttpClient = shopifyNaughtyClientFactory.Make(userId);
                shopifyHttpClient.ShopifyRetriesEnabled = true;
                shopifyHttpClient.ThrowExceptionOnBadHttpStatusCode = true;

                var orderRefreshService = new OrderRefreshService(userId, logger, shopifyHttpClient);
                var RefreshServiceShopifyOrderLimit =
                    Int32.Parse(ConfigurationManager.AppSettings["RefreshServiceShopifyOrderLimit"]);
                orderRefreshService.ShopifyOrderLimit = RefreshServiceShopifyOrderLimit;
                orderRefreshService.Execute();
            }
            catch (Exception e)
            {
                logger.Error(e);
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
