using System.Net;
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
            var json = new { recurring_application_charge = input }.SerializeToJson();

            var request = _requestFactory.HttpPost(ShopifyCredentials, path, json);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            
            return RecurringApplicationCharge.FromDynamic(parent.recurring_application_charge);
        }        

        public virtual RecurringApplicationCharge RetrieveCharge(long id)
        {
            var path = $"/admin/recurring_application_charges/{id}.json";
            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);

            if (clientResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);

            return RecurringApplicationCharge.FromDynamic(parent.recurring_application_charge);
        }

        public virtual RecurringApplicationCharge ActivateCharge(RecurringApplicationCharge input)
        {
            var path = $"/admin/recurring_application_charges/{input.id}/activate.json";
            var json = new { recurring_application_charge = input }.SerializeToJson();

            var request = _requestFactory.HttpPost(ShopifyCredentials, path, json);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);

            return RecurringApplicationCharge.FromDynamic(parent.recurring_application_charge);
        }

        public virtual void CancelCharge(long id)
        {
            var path = $"/admin/recurring_application_charges/{id}.json";
            var request = _requestFactory.HttpDelete(ShopifyCredentials, path);
            _client.ExecuteRequest(request);
        }
    }
}

