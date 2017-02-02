using System;
using Autofac;
using Hangfire;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Processes;


namespace ProfitWise.Batch
{
    class Program
    {
        private const string HangFireBackgroundServiceOption = "1";
        private const string ScheduleInitialShopRefreshOption = "2";
        private const string ScheduleBackgroundSystemJobsOption = "3";

        static void Main(string[] args)
        {
            Console.WriteLine("ProfitWise.Batch v1.0 - Push Automated Commerce LLC");
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("");


            var choice = SolicitUserInput();

            if (choice.Trim() == HangFireBackgroundServiceOption)
            {
                HangFireBackgroundService();
                return;
            }
            if (choice.Trim() == ScheduleBackgroundSystemJobsOption)
            {
                ScheduleBackgroundSystemJobs();
                return;
            }
            if (choice.Trim() == ScheduleInitialShopRefreshOption)
            {
                ScheduleInitialShopRefresh();
                return;
            }

            Console.WriteLine("Invalid option - exiting. Bye!");
        }

        public static void ExitWithAnyKey()
        {
            Console.WriteLine("Hit any key to exit... FIN");
            Console.ReadKey();
        }

        public static string SolicitUserInput()
        {
            Console.WriteLine("Please select from the following options and hit enter");
            Console.WriteLine($"{HangFireBackgroundServiceOption} - Run HangFire Background Service");
            Console.WriteLine($"{ScheduleInitialShopRefreshOption} - Schedule an Initial Shop Refresh");
            Console.WriteLine($"{ScheduleBackgroundSystemJobsOption} - Schedule the default ProfitWise System Jobs");
            Console.WriteLine("");
            return Console.ReadLine();
        }

        public static void ScheduleBackgroundSystemJobs()
        {
            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<HangFireService>();
                service.ScheduleExchangeRateRefresh();
                service.ScheduleSystemCleanupProcess();

                ExitWithAnyKey();
            }
        }

        public static void ScheduleInitialShopRefresh()
        {
            Console.WriteLine("");
            Console.WriteLine("Please enter the User Id:");
            var userId = Console.ReadLine();
            userId = userId == "" ? "ff692d3d-26ef-4a0f-aa90-0e24b4cfe26f" : userId;
            Console.WriteLine($"Using User Id: {userId}");

            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<HangFireService>();
                service.TriggerInitialShopRefresh(userId);
                
            }

            ExitWithAnyKey(); 
        }
        
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

