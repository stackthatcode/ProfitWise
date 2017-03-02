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
    public class RecurringApiRepository : IRecurringApiRepository
    {
        private readonly IHttpClientFacade _client;
        private readonly ShopifyRequestFactory _requestFactory;
        public ShopifyCredentials ShopifyCredentials { get; set; }


        public RecurringApiRepository(
                IHttpClientFacade client,
                ShopifyClientConfig configuration,
                ShopifyRequestFactory requestFactory)
        {
            _client = client;
            _client.Configuration = configuration;
            _requestFactory = requestFactory;
        }

        public virtual RecurringApplicationCharge UpsertCharge(RecurringApplicationCharge input)
        {
            var path = "/admin/recurring_application_charges.json";
            var json = input.SerializeToJson();
            var request = _requestFactory.HttpPost(ShopifyCredentials, json, path);
            var clientResponse = _client.ExecuteRequest(request);

            return clientResponse.Body.DeserializeFromJson<RecurringApplicationCharge>();
        }
        public virtual RecurringApplicationCharge RetrieveCharge(string id)
        {
            var path = $"/admin/recurring_application_charges/#{id}.json";
            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);

            return clientResponse.Body.DeserializeFromJson<RecurringApplicationCharge>();
        }

    }
}

