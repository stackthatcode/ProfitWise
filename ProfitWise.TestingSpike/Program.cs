using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Dapper;
using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;
using Push.Utilities.Security;
using Push.Utilities.Shopify;
using Push.Utilities.Web.Identity;

namespace ProfitWise.TestingSpike
{
    class Program
    {
        static void Main(string[] args)
        {
            TestShopifyApiCalls();
        }

        public static void TestNumberOfPages()
        {
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 0));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 1));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 10));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 11));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 20));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 21));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 30));
            Console.ReadLine();
            // 0 / 10 = 0
            // 1 / 10 = 1
            // 10 / 10 = 1
            // 11 / 10 = 2
            // 20 / 10 = 2
            // 21 / 10 = 3            
        }



        private static void TestEncryption()
        {           
            var inputText =
                "This is all clear now! Now why da hell isn't our sdgkjdslg glkjfklgjfdlgjfdklgjkldfjgljkljl 1234";

            var encryption_key = ConfigurationManager.AppSettings["security_aes_key"];
            var encryption_iv = ConfigurationManager.AppSettings["security_aes_iv"];

            var crypto_service = new EncryptionService(encryption_key, encryption_iv);
            var encrypted = crypto_service.Encrypt(inputText);
            var decrypted = crypto_service.Decrypt(encrypted);
        }

        public class Claim
        {
            public int Id { get; set; }
            public string UserId { get; set; }
            public string ClaimType { get; set; }
            public string ClaimValue { get; set; }
        }


        private static void TestShopifyApiCalls()
        {
            LoggerSingleton.Get = NLoggerImpl.RegistrationFactory("Initializer");
            var logger = LoggerSingleton.Get();

            // STEP #0 - configure Crypto Service
            var encryption_key = ConfigurationManager.AppSettings["security_aes_key"];
            var encryption_iv = ConfigurationManager.AppSettings["security_aes_iv"];
            var crypto_service = new EncryptionService(encryption_key, encryption_iv);
            ShopifyCredentialService.EncryptionService = crypto_service;


            // STEP #1 - Which store are we refereshing...?
            var userId = "20af5482-4fdf-4a9c-9a70-c21319e37936";


            // TODO - package #2 and #3 in a factory or a service 

            // STEP #2 - Get Shopify ProfitWise Credentials   
            var apiKey = ConfigurationManager.AppSettings["shopify_config_apikey"];
            var apiSecret = ConfigurationManager.AppSettings["shopify_config_apisecret"];


            // STEP #3 - I need an Access Token and Store!!!
            var context = ApplicationDbContext.Create();
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            var shopifyCredentialService = new ShopifyCredentialService(userManager);
            var shopifyFromClaims = shopifyCredentialService.Retrieve(userId);

            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(shopifyFromClaims.Message);
            }


            // STEP #5 - Hello World!!!
            var shopifyClient =
                ShopifyHttpClient3.Factory(apiKey, apiSecret, shopifyFromClaims.ShopName, shopifyFromClaims.AccessToken);

            var repository = new OrderRepository(shopifyClient);
            var results = repository.Retrieve();

            foreach (var order in results)
            {
                var message = string.Format("Order found: {0} - {1}", order.Id, order.Email);
                logger.Info(message);
            }

            // STEP #6 - Shit's getting real! Let's cycle through multiple Page
            // TODO - move to configuration
            var page_size = 10;
            var productRepository = new ProductRepository(shopifyClient, logger);
            var allproducts = productRepository.RetrieveAll(page_size);


            // STEP #5 - Save my Orders
            var connectionstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
            connection.Open();
            // Get my Claims
            //var claimQuery = string.Format("SELECT * FROM aspnetuserclaims WHERE Id = {0}", userId);
            //var claims = connection.ExecuteReader(claimQuery, userId);

        }


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

        private static void MySqlTesting()
        {
            var connectionstring = "server=127.0.0.1;uid=root;pwd=sqlBoomba123!@#;database=profitwise;";
            var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
            connection.Open();

            //var query = "SELECT * FROM OrderSkuHistory";
            //var query = @"SELECT * FROM OrderSkuHistory WHERE LineId > 10000 AND LineId < 10043";
            var query = @"INSERT INTO OrderSkuHistory (OrderNumber, ProductSku, Price, CoGS) VALUES ('1312381930', 'ABCDEFG001', 9.00, 7.50);";

            Console.WriteLine("Start... " );
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            
            connection.Execute("START TRANSACTION;");

            var counter = 0;
            while (counter < 50000)
            {
                //Console.WriteLine(DateTime.Now+ " " + counter);
                var result = connection.Execute(query);
                counter++;
            }

            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            connection.Execute("COMMIT;");
            Console.WriteLine("End... " + elapsedTime);
            Console.ReadLine();
        }

        private static void SqlServerTesting()
        {
            var connection = ConnectionFactory.Make();
            var sp_query = @"EXEC Populate";
            connection.Execute(sp_query);


                /*            var query = @"SELECT * FROM OrderSkuHistory WHERE LineId > 1300000 AND LineId < 1300043";



                            Console.WriteLine("Starting... " + DateTime.Now);

                            var counter = 0;
                            while (counter < 1000)
                            {
                            var result =
                                connection
                                    .Query<OrderSkuHistory>(query)
                                    .ToList();

                                Console.WriteLine(DateTime.Now + " " + result.First().Price);
                                counter ++;
                            }

                            Console.WriteLine("End... " + DateTime.Now);

                            Console.ReadLine();
                            */
            }
        }
}
