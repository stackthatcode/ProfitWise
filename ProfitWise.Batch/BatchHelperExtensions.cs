using System;
using Autofac;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Batch
{
    public static class BatchHelperExtensions
    {
        public static void ExecuteInScopeWithErrorLogging(
                    this IContainer autofacContainer, Action<ILifetimeScope> action)
        {
            using (var scope = autofacContainer.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();

                try
                {
                    action(scope);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }
    }
}
