using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Dapper;
using MySql.Data.MySqlClient;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Batch
{
    public class AsyncQuerySimPart2
    {
        private readonly IContainer _container;
        private IPushLogger _logger;

        // Reference Material => https://www.youtube.com/watch?v=DqjIQiZ_ql4

        public AsyncQuerySimPart2(IContainer container)
        {
            _container = container;
            _logger = container.Resolve<IPushLogger>();
        }

        public async void Execute(int workerNumber)
        {
            for (var counter = 0; counter < 10; counter++)
            {
                var timer = new Stopwatch();
                timer.Start();

                _logger.Info($"Worker: {workerNumber} - Starting Query: {counter}");
                
                using (var connection = _container.Resolve<MySqlConnection>())
                { 
                    await connection.QueryAsync(
                        $"SELECT ShopId, ReportedSku FROM TESTVIEW WHERE ShopId = {counter}");
                }

                timer.Stop();
                _logger.Info($"Worker: {workerNumber} - Elapsed Time: {timer.ElapsedMilliseconds}");
            }
        }

        
        //private static void ExecuteAsyncForay(IContainer container)
        //{
        //    var foray = new AsyncForay(container);
        //    foray.CallBigImportantMethod();
        //    foray.CallBigImportantMethod();
        //    foray.CallBigImportantMethod();
        //    container.Resolve<IPushLogger>().Info("Waiting...");
        //}

    }
}
