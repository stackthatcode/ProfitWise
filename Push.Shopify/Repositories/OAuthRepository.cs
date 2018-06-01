using Newtonsoft.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;

namespace Push.Shopify.Repositories
{
    public class OAuthRepository : IShopifyCredentialConsumer
    {
        private readonly ShopifyClientConfig _config;
        private readonly IHttpClientFacade _client;
        private readonly ShopifyRequestFactory _requestFactory;
        private readonly IPushLogger _logger;

        public ShopifyCredentials ShopifyCredentials { get; set; }

        public OAuthRepository(
                ShopifyClientConfig config,
                IHttpClientFacade client, 
                ShopifyRequestFactory requestFactory, 
                IPushLogger logger)
        {
            _config = config;
            _client = client;
            _requestFactory = requestFactory;
            _logger = logger;
        }

        public string RetrieveAccessToken(string code)
        {
            var queryString
                = new QueryStringBuilder()
                    .Add("client_id", ShopifyCredentials.Key)
                    .Add("client_secret", ShopifyCredentials.Secret)
                    .Add("code", code)
                    .ToString();

            var url = $"/admin/oauth/access_token?{queryString}";
            
            var request = _requestFactory.HttpPost(ShopifyCredentials, url, "");
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);

            return parent.access_token;
        }
    }
}
