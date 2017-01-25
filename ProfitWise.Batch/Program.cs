using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap.ConfigureApp();

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


