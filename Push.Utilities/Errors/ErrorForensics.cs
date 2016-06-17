using System;
using Castle.DynamicProxy;
using Push.Utilities.Logging;

namespace Push.Utilities.Errors
{
    public class ErrorForensics : IInterceptor
    {
        private readonly ILogger _logger;

        public ErrorForensics(ILogger logger)
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
                    _logger.Error($"Parameter {counter++}: {arg}");
                }
                throw;
            }
        }
    }
}
