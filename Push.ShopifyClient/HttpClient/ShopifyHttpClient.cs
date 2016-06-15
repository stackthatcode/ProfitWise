using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Push.Utilities.Helpers;

namespace Push.Shopify.HttpClient
{

    public class ShopifyHttpClient : IShopifyHttpClient
    {
        private readonly string _key;
        private readonly string _secret;

        private readonly string _baseurl;
        private readonly string _token;

        private readonly IHttpClient _httpClient;

        public bool ShopifyRetriesEnabled = false;
        public int ShopifyTimeout = 30000;
        public int ShopifyRetryLimit = 3;
        

        // TODO: add Logger for verbose


        public ShopifyHttpClient(IHttpClient httpClient, string shop_domain, string token)
        {
            _baseurl = ShopUrlFromDomain(shop_domain);
            _token = token;
            _httpClient = httpClient;
        }

        public ShopifyHttpClient(IHttpClient httpClient, string shop_domain, string key, string secret)
        {
            _baseurl = ShopUrlFromDomain(shop_domain);
            _key = key;
            _secret = secret;
            _httpClient = httpClient;
            
        }


        public HttpClientResponse HttpGet(string path)
        {
            var request = ShopifyRequestFactory(path);
            request.Method = "GET";

            return ShopifyRetriesEnabled
                ? HttpInvocationWithRetries(request)
                : _httpClient.ProcessRequest(request);
        }


        private HttpClientResponse HttpInvocationWithRetries(HttpWebRequest request)
        {
            for (int counter = 1; counter <= ShopifyRetryLimit; counter++)
            {
                var response = _httpClient.ProcessRequest(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
            }

            var message = string.Format(
                    "Retry limit of {0} reached for HTTP invocation of {1}", 
                    ShopifyRetryLimit, request.RequestUri);
            throw new Exception(message);
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
            req.Timeout = ShopifyTimeout;

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

        public HttpClientResponse HttpGet(string path, bool onlyHttp200WithRetries = false)
        {
            throw new NotImplementedException();
        }
    }
}

