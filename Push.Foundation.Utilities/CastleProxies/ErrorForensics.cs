using Castle.DynamicProxy;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Utilities.CastleProxies
{
    public class ErrorForensics : IInterceptor
    {
        private readonly IPushLogger _pushLogger;

        public ErrorForensics(IPushLogger logger)
        {
            _pushLogger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            // TODO: fix this shit up!
            //try
            //{
            //    invocation.Proceed();
            //}
            //catch (Exception ex)
            //{
            //    _pushLogger.Error(invocation.Method.Name + " threw Exception " + ex.GetType() + " - dumping parameters");
            //    var counter = 1;
            //    foreach (var arg in invocation.Arguments)
            //    {
            //        _pushLogger.Error($"Parameter: {counter++}: {arg}");
            //        //_pushLogger.Error($"{arg.DumpProperties()}");
            //    }
            //    throw;
            //}
        }
    }
}
