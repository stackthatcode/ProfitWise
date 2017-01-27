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
            };
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection", options);

            var backgroundServerOptions = new BackgroundJobServerOptions()
            {
                SchedulePollingInterval = new TimeSpan(0, 0, 0, 1),                
            };

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

        public static void NewBatchStuff()
        {
            using (var container = AutofacRegistration.Build())
            {
                var executionLoops = new List<Task>();
                var counter = 0;

                while (++counter <= 10)
                {
                    var context = new ExecutionLoopContext
                    {
                        TaskId = counter,
                        DelayMilliseconds = 1000,
                    };

                    var loop = Task.Run(() => ExecutionLoop(container, context));
                    executionLoops.Add(loop);
                }

                Task.WaitAll(executionLoops.ToArray());
                Console.WriteLine("ProfitWise.Batch - started...");
                Console.ReadLine();
            }
        }
        
        public static async void ExecutionLoop(IContainer container, ExecutionLoopContext context)
        {
            Console.WriteLine($"New WorkerThread {context.TaskId}");
            while (true)
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    WorkerTaskInner(scope, context);

                    //var random = new Random();
                    await Task.Delay(context.DelayMilliseconds);
                }
            }
        }

        public static async void WorkerTaskInner(ILifetimeScope scope, ExecutionLoopContext context)
        {
            var logger = scope.Resolve<IPushLogger>();
            try
            {
                // ...

                Console.WriteLine($"WorkerThread {context.TaskId} - checking in");
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }
    }

    public class ExecutionLoopContext
    {
        public int DelayMilliseconds { get; set; }
        public int TaskId { get; set; }
    }
}


