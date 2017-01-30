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

        public void SetRandomScopeId()
        {
            var generator = new Random();
            int traceId = generator.Next(1000, 9999);
            this.ScopedPrefix = $"TraceId:{traceId}";
        }


        public void SetScopedPrefix(PwShop shop)
        {
            ScopedPrefix = $"PwShopId: {shop.PwShopId}";
        }

        private string MessageBuilder(string content)
            
        {
            return ScopedPrefix == null ? content : $"{ScopedPrefix} - {content}";
        }

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
