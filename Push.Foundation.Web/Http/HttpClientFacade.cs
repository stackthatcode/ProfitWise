using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Push.Foundation.Utilities.Logging;


namespace Push.Foundation.Web.Http
{
    public class HttpClientFacade : IHttpClientFacade
    {
        private readonly IHttpClient _httpClient;
        private readonly IPushLogger _pushLogger;
        private HttpClientFacadeConfig _configuration;

        private static readonly IDictionary<string, DateTime> 
            _hostLastExecutionTime = new ConcurrentDictionary<string, DateTime>();

        
        public HttpClientFacade(
                IHttpClient httpClient, 
                HttpClientFacadeConfig configuration, 
                IPushLogger logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _pushLogger = logger;
        }

        public HttpClientFacadeConfig Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }

        public virtual HttpClientResponse ExecuteRequest(HttpWebRequest request)
        {
            if (_configuration.RetriesEnabled)
            {
                return HttpInvocationWithRetries(request);
            }
            else
            {
                string message = $"Invoking HTTP GET on {request.RequestUri.AbsolutePath}";
                _pushLogger.Info(message);
                return HttpInvocation(request);
            }
        }

        // NOTE: all HTTP calls must be routed through this method
        private HttpClientResponse HttpInvocation(HttpWebRequest request)
        {
            var hostname = request.RequestUri.Host;
            ProcessIntentionalDelay(hostname);

            _pushLogger.Debug($"Invoking HTTP {request.Method} on {request.RequestUri.AbsoluteUri}");
            _hostLastExecutionTime[hostname] = DateTime.Now;
            var response = _httpClient.ProcessRequest(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new HttpClientResponse { StatusCode = HttpStatusCode.NotFound };
            }
            if (response.StatusCode == (HttpStatusCode)429)
            {
                throw new TooManyHttpRequestsException(response.Body);
            }
            if (response.StatusCode != HttpStatusCode.OK 
                    && response.StatusCode != HttpStatusCode.Created
                    && response.StatusCode != HttpStatusCode.Accepted
                    && _configuration.ThrowExceptionOnBadHttpStatusCode)
            {
                throw new BadHttpStatusCodeException(response.StatusCode, response.Body);
            }
            
            var executionTime = DateTime.Now - _hostLastExecutionTime[hostname];
            _pushLogger.Debug($"Call performance - {executionTime} ms");
            return response;
        }

        private void ProcessIntentionalDelay(string hostname)
        {
            if (_hostLastExecutionTime.ContainsKey(hostname))
            {
                var lastExecutionTime = _hostLastExecutionTime[hostname];
                var timeSinceLastExecution = DateTime.Now - lastExecutionTime;

                var throttlingDelay = new TimeSpan(0, 0, 0, 0, _configuration.ThrottlingDelay);

                if (timeSinceLastExecution < throttlingDelay)
                {
                    var remainingTimeToDelay = throttlingDelay - timeSinceLastExecution;
                    _pushLogger.Debug($"Intentional delay before next call: {remainingTimeToDelay} ms");
                    System.Threading.Thread.Sleep(remainingTimeToDelay);
                }
            }
        }

        private HttpClientResponse HttpInvocationWithRetries(HttpWebRequest request)
        {
            var counter = 1;

            while (true)
            {
                try
                {
                    var response = HttpInvocation(request);
                    return response;
                }
                catch (BadHttpStatusCodeException e)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _pushLogger.Error(ex);
                    counter++;

                    if (counter > _configuration.RetryLimit)
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


