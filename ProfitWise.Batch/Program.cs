using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
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
            using (var scope = container.BeginLifetimeScope())
            {
                //var userId = "d56850fb-3fe7-4c66-a59d-20f755f5f1f4";
                var userId = "57f0da58-6e74-41d5-90a9-736d09aa3b2f";

                var refreshProcess = scope.Resolve<RefreshProcess>();
                refreshProcess.Execute(userId);
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


