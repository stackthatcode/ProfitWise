using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Processes;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap.ConfigureApp();
            HangFireBackgroundServiceTest();
        }
        

        public static void HangFireBackgroundServiceTest()
        {
            LogProvider.SetCurrentLogProvider(new HangFireLogProvider());

            var options = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(1),
                PrepareSchemaIfNecessary = false,                
            };
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection", options);

            var backgroundServerOptions 
                = new BackgroundJobServerOptions()
                    {
                        SchedulePollingInterval = new TimeSpan(0, 0, 0, 1),                
                    };

            BackgroundJob.Delete("50");

            using (var server = new BackgroundJobServer(backgroundServerOptions))
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }


        public static void StandaloneRefreshProcess()
        {
            Bootstrap.ConfigureApp();

            using (var container = AutofacRegistration.Build())
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = container.Resolve<IPushLogger>();
                try
                {
                    //var userId = "d56850fb-3fe7-4c66-a59d-20f755f5f1f4";
                    var userId = "ff692d3d-26ef-4a0f-aa90-0e24b4cfe26f";

                    //var currencyProcess = scope.Resolve<CurrencyProcess>();
                    //currencyProcess.Execute();

                    var refreshProcess = scope.Resolve<ShopRefreshProcess>();
                    refreshProcess.Execute(userId);
                }
                catch (Exception e)
                {
                    logger.Error(e);

                    // Add notification service call here
                    logger.Fatal(e.Message);
                }

                Console.ReadLine();
            }
        }
    }    
}

