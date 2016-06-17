using Castle.DynamicProxy;
using Push.Utilities.Logging;

namespace ProfitWise.Batch._TempHoldingCell
{
    public class LoggingInterceptor : IInterceptor
    {
        private readonly ILogger _logger;

        public LoggingInterceptor(ILogger logger)
        {
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            _logger.Info("About to execute method " + invocation.Method.Name + " on instance of " + invocation.TargetType);
            invocation.Proceed();
            _logger.Info("Just finished executing method " + invocation.Method.Name + " on instance of " + invocation.TargetType);
        }
    }    
}

