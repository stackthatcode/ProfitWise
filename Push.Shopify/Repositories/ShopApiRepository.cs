using Autofac.Extras.DynamicProxy2;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
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
        private readonly IPushLogger _logger;
        public ShopifyCredentials ShopifyCredentials { get; set; }


        public ShopApiRepository(
                IHttpClientFacade client,
                ShopifyClientConfig configuration,
                ShopifyRequestFactory requestFactory,
                IPushLogger logger)
        {
            _client = client;
            _client.Configuration = configuration;
            _requestFactory = requestFactory;
            _logger = logger;
        }

        public virtual Shop Retrieve()
        {
            var path = "/admin/api/2019-10/shop.json";                       
            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);

            _logger.Info($"/admin/api/2019-10/shop.json response body: {clientResponse.Body}");

            var output = Shop.MakeFromJson(clientResponse.Body);
            return output;
        }

    }
}

