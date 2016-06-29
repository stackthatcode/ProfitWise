using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Batch
{
    public class AsyncForay
    {
        private readonly IContainer _container;

        // Reference Material => https://www.youtube.com/watch?v=DqjIQiZ_ql4

        public AsyncForay(IContainer container)
        {
            _container = container;
        }

        public async void CallBigImportantMethod()
        {
            using (var scope = _container.BeginLifetimeScope())
            {
                var result = await BigImportantMethodAsync();
                var logger = scope.Resolve<IPushLogger>();
                logger.Info("Result of BigImportantMethod: " + result);
            }
        }


        // Hey, here's an async method
        public Task<string> BigImportantMethodAsync()
        {
            return Task.Factory.StartNew(() => BigImportantMethod());
        }

        public string BigImportantMethod()
        {
            Thread.Sleep(5000);
            return "Ok, I'm done!!!";
        }


    }
}
