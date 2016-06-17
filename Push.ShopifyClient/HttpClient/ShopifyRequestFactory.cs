using System;
using System.Net;
using Push.Utilities.Helpers;

namespace Push.Shopify.HttpClient
{
    public class ShopifyRequestFactory
    {
        private readonly ShopifyHttpClientConfig _configuration;

        public ShopifyRequestFactory(ShopifyHttpClientConfig configuration)
        {
            _configuration = configuration;
        }


        public HttpWebRequest HttpGet(ShopifyCredentials credentials, string path)
        {
            var request = FactoryWorker(credentials, path);
            request.Method = "GET";
            return request;
        }

        private HttpWebRequest FactoryWorker(ShopifyCredentials credentials, string path)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var url = credentials.ShopBaseUrl + path;
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = _configuration.ShopifyHttpTimeout;

            if (credentials.AccessToken.IsNullOrEmpty())
            {
                // This is what we would use for 3D Universe Automation only
                var credentialCache = new CredentialCache();
                credentialCache.Add(new Uri(url), "Basic", new NetworkCredential(credentials.Key, credentials.Secret));
                req.Credentials = credentialCache;
            }
            else
            {
                // This is normal Shopify Owner plug-in authentication
                req.Headers["X-Shopify-Access-Token"] = credentials.AccessToken;
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

