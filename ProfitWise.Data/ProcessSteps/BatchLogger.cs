using System;
using ProfitWise.Data.Model.Shop;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.ProcessSteps
{

    // More of a helper class than anything else...
    public class BatchLogger
    {
        private readonly IPushLogger _logger;

        public string ScopedPrefix { get; set; }

        public BatchLogger(IPushLogger logger)
        {
            _logger = logger;
            SetRandomScopeId();
        }

        static readonly Random Generator = new Random();

        public void SetRandomScopeId()
        {
            int traceId = Generator.Next(1000, 9999);
            this.ScopedPrefix = $"TraceId:{traceId}";
        }        

        private string MessageBuilder(string content)
            
        {
            return ScopedPrefix == null ? content : $"{ScopedPrefix} - {content}";
        }

        public bool IsTraceEnabled => _logger.IsTraceEnabled;

        public void Trace(string message)
        {
            _logger.Trace(MessageBuilder(message));
        }

        public void Debug(string message)
        {
            _logger.Debug(MessageBuilder(message));
        }

        public void Info(string message)
        {
            _logger.Info(MessageBuilder(message));
        }

        public void Warn(string message)
        {
            _logger.Warn(MessageBuilder(message));
        }

        public void Error(string message)
        {
            _logger.Error(MessageBuilder(message));
        }

        public void Error(Exception exception)
        {
            _logger.Error(MessageBuilder("Error occurred"));
            _logger.Error(exception);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(MessageBuilder(message));
        }

        public void Fatal(Exception exception)
        {
            _logger.Fatal(MessageBuilder("FATAL Error occurred"));
            _logger.Fatal(exception);
        }
    }
}
