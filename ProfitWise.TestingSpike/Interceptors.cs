using System;
using Castle.DynamicProxy;
using Push.Utilities.Logging;

namespace ProfitWise.Batch
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
    


    public class ErrorForensicsInterceptor : IInterceptor
    {
        private readonly ILogger _logger;

        public ErrorForensicsInterceptor(ILogger logger)
        {
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                _logger.Error(invocation.Method.Name + " threw Exception " + ex.GetType() + " - dumping parameters");
                var counter = 1;
                foreach (var arg in invocation.Arguments)
                {
                    _logger.Error(string.Format("Parameter {0}: {1}", counter++, arg));
                }
                throw;
            }
        }
    }
}
