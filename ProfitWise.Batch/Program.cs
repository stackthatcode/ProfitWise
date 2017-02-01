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
            //var userId = "ff692d3d-26ef-4a0f-aa90-0e24b4cfe26f";
            //OrderRefreshProcess(userId);

            //var userId = "ff692d3d-26ef-4a0f-aa90-0e24b4cfe26f";
            //ScheduleInitialShopRefresh(userId);            

            //ScheduleBackgroundSystemJobs();

            HangFireBackgroundService();
        }


        public static void ScheduleBackgroundSystemJobs()
        {
            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<HangFireService>();
                service.ScheduleExchangeRateRefresh();
                service.ScheduleSystemCleanupProcess();

                Console.ReadLine();
            }
        }

        public static void ScheduleInitialShopRefresh(string userId)
        {
            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<HangFireService>();
                service.TriggerInitialShopRefresh(userId);
                Console.ReadLine();
            }
        }
        

        // Aleks is working on this - go ahead and ignore
        public static void HangFireBackgroundService()
        {
            Bootstrapper.ConfigureApp(true);
            var backgroundServerOptions 
                = new BackgroundJobServerOptions()
                    {
                        SchedulePollingInterval = new TimeSpan(0, 0, 0, 1),   
                        Queues = QueueChannel.All,
                    };

            using (var server = new BackgroundJobServer(backgroundServerOptions))
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }
        
        public static void OrderRefreshProcess(string userId)
        {
            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {   
                var refreshProcess = scope.Resolve<ShopRefreshProcess>();
                refreshProcess.RoutineShopRefresh(userId);

                Console.ReadLine();
            }
        }
    }
}

