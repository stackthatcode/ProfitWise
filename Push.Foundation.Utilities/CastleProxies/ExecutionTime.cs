using System;
using System.Diagnostics;
using Castle.DynamicProxy;
using Push.Foundation.Utilities.Logging;
using Push.Utilities.Helpers;

namespace Push.Foundation.Utilities.CastleProxies
{
    public class ExecutionTime : IInterceptor
    {
        private readonly IPushLogger _pushLogger;

        public ExecutionTime(IPushLogger logger)
        {
            _pushLogger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            invocation.Proceed();

            // Lovely but unnecessarily complicated!
            //var proxyTarget = (invocation.Proxy as IProxyTargetAccessor);
            //var typeName = (proxyTarget != null)
            //    ? proxyTarget.DynProxyGetTarget().GetType().Name
            //    : invocation.InvocationTarget.GetType().Name;

            TimeSpan ts = stopWatch.Elapsed;
            _pushLogger.Info($"{invocation.TargetType.Name}.{invocation.Method.Name} - metered execution time {ts.ToFormattedString()}");
        }
    }
}
