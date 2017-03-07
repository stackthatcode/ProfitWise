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

            return clientResponse.Body.ToWebhook();
        }


        public Webhook Retrieve(long id)
        {
            var path = $"/admin/webhooks/{id}.json";
            var httpRequest = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(httpRequest);

            return clientResponse.Body.ToWebhook();
        }
    }
}

