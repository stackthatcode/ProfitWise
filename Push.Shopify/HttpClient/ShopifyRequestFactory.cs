using System;
using System.IO;
using System.Net;
using System.Text;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Web.Http;

namespace Push.Shopify.HttpClient
{
    public class ShopifyRequestFactory
    {
        private readonly HttpClientFacadeConfig _configuration;

        public ShopifyRequestFactory(HttpClientFacadeConfig configuration)
        {
            _configuration = configuration;
        }

        public HttpWebRequest HttpGet(ShopifyCredentials credentials, string path)
        {
            var request = FactoryWorker(credentials, path);
            request.Method = "GET";
            return request;
        }

        public HttpWebRequest HttpPost(ShopifyCredentials credentials, string path, string content)
        {
            var request = FactoryWorker(credentials, path);
            request.Method = "POST";

            var byteArray = Encoding.ASCII.GetBytes(content);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            return request;
        }

        public HttpWebRequest HttpPut(ShopifyCredentials credentials, string path, string content)
        {
            var request = FactoryWorker(credentials, path);
            request.Method = "PUT";

            var byteArray = Encoding.ASCII.GetBytes(content);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            return request;
        }

        public HttpWebRequest HttpDelete(ShopifyCredentials credentials, string path)
        {
            var request = FactoryWorker(credentials, path);
            request.Method = "DELETE";
            return request;
        }

        private HttpWebRequest FactoryWorker(ShopifyCredentials credentials, string path)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var url = credentials.ShopBaseUrl + path;
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = _configuration.Timeout;

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
    }
}

