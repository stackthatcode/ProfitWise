using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Push.Shopify.HttpClient
{
    public class ShopifyRequestFactory
    {
        public Re

        public virtual HttpClientResponse HttpGet(string path)
        {
            var request = ShopifyRequestFactory(path);
            request.Method = "GET";
            return request;
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
    }
}
