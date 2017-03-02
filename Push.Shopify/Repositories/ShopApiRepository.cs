using Autofac.Extras.DynamicProxy2;
using Newtonsoft.Json;
using Push.Foundation.Web.Http;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Model;

namespace Push.Shopify.Repositories
{
    [Intercept(typeof(ShopifyCredentialRequired))]
    public class ShopApiRepository : IShopifyCredentialConsumer, IShopApiRepository
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

            var output = Shop.MakeFromJson(clientResponse.Body);
            return output;
        }
        
    }
}

