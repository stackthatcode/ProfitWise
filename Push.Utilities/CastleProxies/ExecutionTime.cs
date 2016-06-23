using System;
using System.Diagnostics;
using Castle.DynamicProxy;
using Push.Utilities.General;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;

namespace Push.Utilities.CastleProxies
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

            TimeSpan ts = stopWatch.Elapsed;

            // Lovely but unnecessarily complicated!
            //var proxyTarget = (invocation.Proxy as IProxyTargetAccessor);
            //var typeName = (proxyTarget != null)
            //    ? proxyTarget.DynProxyGetTarget().GetType().Name
            //    : invocation.InvocationTarget.GetType().Name;

            _pushLogger.Info($"{invocation.TargetType.Name}.{invocation.Method.Name} - metered execution time {ts.ToFormattedString()}");
        }
    }
}
