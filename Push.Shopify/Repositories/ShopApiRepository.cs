using Autofac.Extras.DynamicProxy2;
using Newtonsoft.Json;
using Push.Foundation.Web.Http;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace Push.Shopify.Repositories
{
    [Intercept(typeof(ShopifyCredentialRequired))]
    public class ShopApiRepository : IShopifyCredentialConsumer
    {
        private readonly IHttpClientFacade _client;
        private readonly ShopifyRequestFactory _requestFactory;
        public ShopifyCredentials ShopifyCredentials { get; set; }


        public ShopApiRepository(
                IHttpClientFacade client,
                ShopifyClientConfig configuration,
                ShopifyRequestFactory requestFactory)
        {
            _client = client;
            _client.Configuration = configuration;
            _requestFactory = requestFactory;
        }

        public virtual Shop Retrieve()
        {
            var path = "/admin/shop.json";
                       
            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var output = new Shop();
            output.Id = parent.shop.id;
            output.Currency = parent.shop.currency;
            return output;
        }
        
    }
}

