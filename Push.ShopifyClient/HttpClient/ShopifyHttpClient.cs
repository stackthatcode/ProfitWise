using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;

namespace Push.Shopify.HttpClient
{

    public class ShopifyHttpClient : IShopifyHttpClient
    {
        private readonly string _key;
        private readonly string _secret;
        private readonly string _baseurl;
        private readonly string _token;

        private readonly string _shopDomain;

        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        //public bool ThrowExceptionOnNonHttp200 = true;

        public int ShopifyRetryLimit { get; set; }
        public int ShopifyHttpTimeout { get; set; }
        public int ShopifyThrottlingDelay { get; set; }
        public bool ShopifyRetriesEnabled { get; set; }

        public bool ThrowExceptionOnBadHttpStatusCode { get; set; }


        private static readonly 
            IDictionary<string, DateTime> _shopLastExecutionTime 
                    = new ConcurrentDictionary<string, DateTime>();


        public ShopifyHttpClient(IHttpClient httpClient, ILogger logger, string shop_domain, string token)
        {
            _httpClient = httpClient;
            _logger = logger;

            _baseurl = ShopUrlFromDomain(shop_domain);
            _token = token;
            _shopDomain = shop_domain;

            ConfigureDefaultSettings();
        }

        public ShopifyHttpClient(IHttpClient httpClient, ILogger logger, string shop_domain, string key, string secret)
        {
            _baseurl = ShopUrlFromDomain(shop_domain);
            _key = key;
            _secret = secret;

            _httpClient = httpClient;
            _logger = logger;

            ConfigureDefaultSettings();
        }

        private void ConfigureDefaultSettings()
        {
            ShopifyRetryLimit = 3;
            ShopifyRetriesEnabled = false;
            ShopifyHttpTimeout = 30000;
            ShopifyThrottlingDelay = 500;
        }

        public HttpClientResponse HttpGet(string path)
        {
            var request = ShopifyRequestFactory(path);
            request.Method = "GET";

            if (ShopifyRetriesEnabled)
            {
                return HttpInvocationWithRetries(request);
            }
            else
            {
                string message = String.Format("Invoking HTTP GET on {0}", path);
                _logger.Debug(message);
                return HttpInvocationWithThrottling(request);
            }
        }

        // NOTE: all HTTP calls must be routed through this method
        private HttpClientResponse HttpInvocationWithThrottling(HttpWebRequest request)
        {
            if (_shopLastExecutionTime.ContainsKey(_shopDomain))
            {
                var lastExecutionTime = _shopLastExecutionTime[_shopDomain];

                var timeSinceLastExecution = DateTime.Now - lastExecutionTime;

                if (timeSinceLastExecution.Milliseconds < ShopifyThrottlingDelay)
                {
                    var remainingTimeToDelay = ShopifyThrottlingDelay - timeSinceLastExecution.Milliseconds;
                    System.Threading.Thread.Sleep(timeSinceLastExecution);
                    _logger.Debug(string.Format("Intentional delay before next call: {0} ms", remainingTimeToDelay));
                }
            }

            _logger.Debug(String.Format("Invoking HTTP GET on {0}", request.RequestUri.AbsoluteUri));
            _shopLastExecutionTime[_shopDomain] = DateTime.Now;

            var response = _httpClient.ProcessRequest(request);

            if (response.StatusCode != HttpStatusCode.OK && ThrowExceptionOnBadHttpStatusCode)
            {
                throw new BadShopifyHttpStatusCodeException(response.StatusCode);
            }

            var executionTime = DateTime.Now - _shopLastExecutionTime[_shopDomain];
            _logger.Debug(string.Format("Call performance - {0} ms", executionTime));

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
                    _logger.Error(ex);
                    counter++;

                    if (counter > ShopifyRetryLimit)
                    {
                        _logger.Fatal("Retry Limit has been exceeded... throwing exception");
                        throw;
                    }
                    else
                    {
                        _logger.Debug(String.Format("Encountered an exception. Retrying invocation..."));
                    }
                }
            }
        }


        private static string ShopUrlFromDomain(string domain)
        {
            return string.Format("https://{0}", domain);
        }

        private HttpWebRequest ShopifyRequestFactory(string path)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var url = _baseurl + path;
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = ShopifyHttpTimeout;

            if (_token.IsNullOrEmpty())
            {
                // This is what we would use for 3D Universe Automation only
                var credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(url), "Basic", new NetworkCredential(_key, _secret));
                req.Credentials = credentialCache;
            }
            else
            {
                // This is normal Shopify Owner plug-in authentication
                req.Headers["X-Shopify-Access-Token"] = _token;
            }
            return req;
        }


        //public string HttpPut(string path, string json)
        //{
        //    var request = RequestFactory(path);
        //    request.Method = "PUT";
        //    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
        //    {
        //        streamWriter.Write(json);
        //    }
        //    return ProcessRequest(request);
        //}

        //public string HttpPost(string path, string json)
        //{
        //    var request = RequestFactory(path);
        //    request.Method = "POST";
        //    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
        //    {
        //        streamWriter.Write(json);
        //    }
        //    return ProcessRequest(request);
        //}
    }
}

