using Castle.DynamicProxy;
using Push.Utilities.Logging;

namespace ProfitWise.Batch._TempHoldingCell
{
    public class LoggingInterceptor : IInterceptor
    {
        private readonly IPushLogger _pushLogger;

        public LoggingInterceptor(IPushLogger logger)
        {
            _pushLogger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            _pushLogger.Info("About to execute method " + invocation.Method.Name + " on instance of " + invocation.TargetType);
            invocation.Proceed();
            _pushLogger.Info("Just finished executing method " + invocation.Method.Name + " on instance of " + invocation.TargetType);
        }
    }    
}

