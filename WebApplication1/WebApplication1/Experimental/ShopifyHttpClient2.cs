using System;
using System.IO;
using System.Net;


namespace WebApplication1.Experimental
{
    public class ShopifyHttpClient2
    {
        private readonly string _key;
        private readonly string _secret;

        private readonly string _baseurl;
        private readonly string _token;

        public ShopifyHttpClient2(string baseurl, string key, string secret, string token)
        {
            _baseurl = baseurl;
            _token = token;
            _key = key;
            _secret = secret;
        }

        public string HttpGet(string path)
        {
            var request = RequestFactory(path);
            request.Method = "GET";
            return ProcessRequest(request);
        }

        public string HttpPut(string path, string json)
        {
            var request = RequestFactory(path);
            request.Method = "PUT";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
            }
            return ProcessRequest(request);
        }

        public string HttpPost(string path, string json)
        {
            var request = RequestFactory(path);
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
            }
            return ProcessRequest(request);
        }


        private string ProcessRequest(HttpWebRequest request)
        {
            using (var resp = (HttpWebResponse)request.GetResponse())
            {
                if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.Created)
                {
                    string message = String.Format("Call failed. Received HTTP {0}", resp.StatusCode);
                    throw new ApplicationException(message);
                }

                var sr = new StreamReader(resp.GetResponseStream());
                return sr.ReadToEnd();
            }
        }

        private HttpWebRequest RequestFactory(string path)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            //ServicePointManager.Expect100Continue = true;
            var url = _baseurl + path;

            //var credentialCache = new CredentialCache();
            //credentialCache.Add(new Uri(url), "Basic", new NetworkCredential(_key, _secret));

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Headers["X-Shopify-Access-Token"] = _token;
            //req.ContentType = "application/json";

            //req.Credentials = credentialCache;
            //req.PreAuthenticate = true;
            return req;
        }
    }
}

