using System;
using System.IO;
using System.Net;

namespace Push.Utilities.Shopify
{
    public class ShopifyHttpClient3
    {
        private readonly string _key;
        private readonly string _secret;

        private readonly string _baseurl;
        private readonly string _token;

        public static string ShopUrl(string domain)
        {
            return string.Format("https://{0}", domain);
        }

        public static ShopifyHttpClient3 Factory(
                string shopify_config_apikey,
                string shopify_config_apisecret,
                string shop_domain, 
                string access_token)
        {
            var shopurl = ShopUrl(shop_domain);

            var shopifyClient =
                new ShopifyHttpClient3(
                    shopurl,
                    shopify_config_apikey,
                    shopify_config_apisecret,
                    access_token);

            return shopifyClient;
        }


        public ShopifyHttpClient3(string baseurl, string key, string secret, string token)
        {
            _baseurl = baseurl;
            _token = token;
            _key = key;
            _secret = secret;
        }

        public ShopifyClientResponse HttpGet(string path)
        {
            var request = RequestFactory(path);
            request.Method = "GET";
            return ProcessRequest(request);
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


        private ShopifyClientResponse ProcessRequest(HttpWebRequest request)
        {
            HttpWebResponse resp;
            try
            {
                using (resp = (HttpWebResponse) request.GetResponse())
                {
                    var sr = new StreamReader(resp.GetResponseStream());
                    return new ShopifyClientResponse()
                    {
                        StatusCode = resp.StatusCode,
                        ResponseBody = sr.ReadToEnd()
                    };
                }
            }
            catch (WebException e)
            {
                return new ShopifyClientResponse()
                {
                    ResponseBody = "fail!",
                    //StatusCode = resp.StatusCode
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        private HttpWebRequest RequestFactory(string path)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var url = _baseurl + path;
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Headers["X-Shopify-Access-Token"] = _token;
            return req;
        }
    }
}

