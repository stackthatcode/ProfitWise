using System;
using Autofac;
using Hangfire;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Processes;


namespace ProfitWise.Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.ConfigureApp();
            HangFireBackgroundService();

            //HangFireScheduleTest();
            //TestOrderCreator.Execute();
        }

        public static void HangFireScheduleTest()
        {
            string jobNumberOne = "Job1", jobNumberTwo = "Job2";
            string userId = "f4a8b3bb-2aec-4c2a-ab49-ba90bd047273";

            RecurringJob.AddOrUpdate<ShopRefreshProcess>(
                jobNumberOne, x => x.RoutineShopRefresh(userId), Cron.Minutely, queue: Queues.RoutineShopRefresh);

            RecurringJob.AddOrUpdate<ShopRefreshProcess>(
                jobNumberTwo, x => x.RoutineShopRefresh(userId), Cron.Minutely, queue: Queues.RoutineShopRefresh);

            //service.KillRecurringJob("ShopRefreshProcess:f4a8b3bb-2aec-4c2a-ab49-ba90bd047273");
        }

        // Aleks is working on this - go ahead and ignore
        public static void HangFireBackgroundService()
        {
            var backgroundServerOptions 
                = new BackgroundJobServerOptions()
                    {
                        SchedulePollingInterval = new TimeSpan(0, 0, 0, 1),   
                        Queues = new [] { Queues.InitialShopRefresh, Queues.RoutineShopRefresh },
                    };

            using (var server = new BackgroundJobServer(backgroundServerOptions))
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }
        
        public static void StandaloneRefreshProcess(IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
            {                
                //var userId = "d56850fb-3fe7-4c66-a59d-20f755f5f1f4";
                var userId = "ff692d3d-26ef-4a0f-aa90-0e24b4cfe26f";

                //var currencyProcess = scope.Resolve<CurrencyProcess>();
                //currencyProcess.Execute();

                var refreshProcess = scope.Resolve<ShopRefreshProcess>();
                refreshProcess.RoutineShopRefresh(userId);

                Console.ReadLine();
            }
        }

    }
}

