using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Dapper;
using MySql.Data.MySqlClient;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Batch
{
    public class AsyncQuerySimulation
    {
        public static void Launch(IContainer container)
        {
            var simulator = new AsyncQuerySimPart2(container);
            for (var i = 0; i < 100; i++)
            {
                var workerNumber = i;
                Task.Factory.StartNew(() => simulator.Execute(workerNumber));
            }

            container.Resolve<IPushLogger>().Info("I launched all my threads!!!");
        }


        public static async void Execute(IContainer container)
        {
            var connection = container.Resolve<MySqlConnection>();
            var logger = container.Resolve<IPushLogger>();
            logger.Info("Starting parallel execution...");

            var tasks = Enumerable.Range(1, 100).Select(
                async x =>
                {
                    var timer = new Stopwatch();
                    timer.Start();
                    logger.Info($"Starting Query {x}");
                    await connection.QueryAsync(
                        $"SELECT ShopId, CreatedAt, ReportedSku, LineTotal FROM TESTVIEW WHERE ShopId = {x}");

                    timer.Stop();
                    logger.Info($"{timer.ElapsedMilliseconds} for Query {x}");
                });

            await Task.WhenAll(tasks);
            logger.Info("All done!");
        }

        public static void ExecuteAlternate(IContainer container)
        {
            var logger = container.Resolve<IPushLogger>();
            logger.Info("Starting parallel execution...");
            var tasks = Enumerable.Range(1, 100);

            Parallel.ForEach(tasks,
                new ParallelOptions { MaxDegreeOfParallelism = 100 },
                x =>
            {
                using (var scope = container.BeginLifetimeScope())
                using (var connection = scope.Resolve<MySqlConnection>())
                {
                    for (var counter = 0; counter < 10; counter++)
                    {
                        var timer = new Stopwatch();
                        timer.Start();

                        connection.Query(
                            $"SELECT ShopId, CreatedAt, ReportedSku, LineTotal FROM TESTVIEW WHERE ShopId = {x}");

                        timer.Stop();
                        logger.Info($"Worker {x} - Executed Query: {counter} - Execution Time: {timer.ElapsedMilliseconds}");
                    }
                }
            });

            logger.Info("All done!");
        }
    }
}
