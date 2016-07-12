using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Push.Foundation.Utilities.Logging;


namespace Push.Shopify.HttpClient
{
    public class ShopifyHttpClient : IShopifyHttpClient
    {
        private readonly IHttpClient _httpClient;
        private readonly ShopifyHttpClientConfig _configuration;
        private readonly IPushLogger _pushLogger;


        private static readonly 
            IDictionary<string, DateTime> _shopLastExecutionTime 
                    = new ConcurrentDictionary<string, DateTime>();

        
        public ShopifyHttpClient(IHttpClient httpClient, ShopifyHttpClientConfig configuration, IPushLogger logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _pushLogger = logger;
        }

        public virtual HttpClientResponse ExecuteRequest(HttpWebRequest request)
        {
            if (_configuration.ShopifyRetriesEnabled)
            {
                return HttpInvocationWithRetries(request);
            }
            else
            {
                string message = $"Invoking HTTP GET on {request.RequestUri.AbsolutePath}";
                _pushLogger.Info(message);
                return HttpInvocationWithThrottling(request);
            }
        }

        // NOTE: all HTTP calls must be routed through this method
        private HttpClientResponse HttpInvocationWithThrottling(HttpWebRequest request)
        {
            var hostname = request.RequestUri.Host;

            if (_shopLastExecutionTime.ContainsKey(hostname))
            {
                var lastExecutionTime = _shopLastExecutionTime[hostname];
                var timeSinceLastExecutionTimeSpan = DateTime.Now - lastExecutionTime;

                var shopifyThrottlingDelayTimeSpan = new TimeSpan(0, 0, 0, 0, _configuration.ShopifyThrottlingDelay);
                
                if (timeSinceLastExecutionTimeSpan < shopifyThrottlingDelayTimeSpan)
                {
                    var remainingTimeToDelay = shopifyThrottlingDelayTimeSpan - timeSinceLastExecutionTimeSpan;
                    _pushLogger.Info($"Intentional delay before next call: {remainingTimeToDelay} ms");
                    System.Threading.Thread.Sleep(remainingTimeToDelay);
                }
            }

            _pushLogger.Info($"Invoking HTTP GET on {request.RequestUri.AbsoluteUri}");
            _shopLastExecutionTime[hostname] = DateTime.Now;

            var response = _httpClient.ProcessRequest(request);

            // TODO => work in the logic to catch 429's and retry

            if (response.StatusCode != HttpStatusCode.OK && _configuration.ThrowExceptionOnBadHttpStatusCode)
            {
                throw new BadShopifyHttpStatusCodeException(response.StatusCode);
            }

            var executionTime = DateTime.Now - _shopLastExecutionTime[hostname];
            _pushLogger.Debug(string.Format("Call performance - {0} ms", executionTime));

            return response;
        }

        private HttpClientResponse HttpInvocationWithRetries(HttpWebRequest request)
        {
            var counter = 1;

            while (true)
            {
                try
                {
                    var response = HttpInvocationWithThrottling(request);
                    return response;
                }
                catch (BadShopifyHttpStatusCodeException e)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _pushLogger.Error(ex);
                    counter++;

                    if (counter > _configuration.ShopifyRetryLimit)
                    {
                        _pushLogger.Fatal("Retry Limit has been exceeded... throwing exception");
                        throw;
                    }
                    else
                    {
                        _pushLogger.Debug(String.Format("Encountered an exception. Retrying invocation..."));
                    }
                }
            }
        }
    }
}


