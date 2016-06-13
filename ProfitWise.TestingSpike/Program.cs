using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Dapper;
using Hangfire;
using Push.Utilities.Logging;

namespace ProfitWise.TestingSpike
{
    class Program
    {
        static void Main(string[] args)
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


        private static void DoStuff()
        {
            System.Threading.Thread.Sleep(5000);
        }


        private static void MySqlTesting()
        {
            var connectionstring = "server=127.0.0.1;uid=root;pwd=sqlBoomba123!@#;database=profitwise;";
            var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
            connection.Open();

            //var query = "SELECT * FROM OrderSkuHistory";
            //var query = @"SELECT * FROM OrderSkuHistory WHERE LineId > 10000 AND LineId < 10043";
            var query = @"    INSERT INTO OrderSkuHistory (OrderNumber, ProductSku, Price, CoGS) VALUES ('1312381930', 'ABCDEFG001', 9.00, 7.50);";

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
