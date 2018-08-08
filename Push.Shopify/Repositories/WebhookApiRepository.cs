using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Web;
using Autofac.Extras.DynamicProxy2;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Web.Http;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Model;

namespace Push.Shopify.Repositories
{
    [Intercept(typeof(ShopifyCredentialRequired))]
    public class WebhookApiRepository : IShopifyCredentialConsumer, IWebhookApiRepository
    {
        private readonly IHttpClientFacade _client;
        private readonly ShopifyRequestFactory _requestFactory;
        public ShopifyCredentials ShopifyCredentials { get; set; }


        public WebhookApiRepository(
                IHttpClientFacade client,
                ShopifyClientConfig configuration,
                ShopifyRequestFactory requestFactory)
        {
            _client = client;
            _client.Configuration = configuration;
            _requestFactory = requestFactory;
        }

        public Webhook Subscribe(Webhook request)
        {
            var path = "/admin/webhooks.json";
            var json = new
            {
                webhook = new
                {
                    topic = request.Topic,
                    address = request.Address,
                    format = request.Format,
                }
            };

            var content = json.SerializeToJson();
            var httpRequest = _requestFactory.HttpPost(ShopifyCredentials, path, content);
            var clientResponse = _client.ExecuteRequest(httpRequest);

            return clientResponse.Body.ToSingleWebhook();
        }

        public Webhook UpdateAddress(Webhook request)
        {
            var path = $"/admin/webhooks/{request.Id}.json";
            var json = new
            {
                webhook = new
                {
                    id = request.Id,
                    address = request.Address,
                }
            };

            var content = json.SerializeToJson();
            var httpRequest = _requestFactory.HttpPut(ShopifyCredentials, path, content);
            var clientResponse = _client.ExecuteRequest(httpRequest);

            return clientResponse.Body.ToSingleWebhook();
        }        

        public Webhook Retrieve(string topic, string address)
        {
            var encodedTopic = HttpUtility.UrlEncode(topic);
            var encodedAddress = HttpUtility.UrlEncode(address);

            var path = $"/admin/webhooks.json?address={encodedAddress}&topic={encodedTopic}";
            var httpRequest = _requestFactory.HttpGet(ShopifyCredentials, path);

            var clientResponse = _client.ExecuteRequest(httpRequest);
            if (clientResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var webhooks = clientResponse.Body.ToMultipleWebhooks();
            return webhooks.FirstOrDefault();
        }

        public List<Webhook> RetrieveAll()
        {
            var path = $"/admin/webhooks.json";
            var httpRequest = _requestFactory.HttpGet(ShopifyCredentials, path);

            var clientResponse = _client.ExecuteRequest(httpRequest);
            if (clientResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return clientResponse.Body.ToMultipleWebhooks();
        }

        public Webhook Retrieve(string topic)
        {
            var encodedTopic = HttpUtility.UrlEncode(topic);
            var path = $"/admin/webhooks.json?topic={encodedTopic}";
            var httpRequest = _requestFactory.HttpGet(ShopifyCredentials, path);

            var clientResponse = _client.ExecuteRequest(httpRequest);
            if (clientResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var webhooks = clientResponse.Body.ToMultipleWebhooks();
            return webhooks.FirstOrDefault();
        }

        public void Delete(long webHookId)
        {
            var path = $"/admin/webhooks/{webHookId}.json";
            var httpRequest = _requestFactory.HttpDelete(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(httpRequest);
        }
    }
}

