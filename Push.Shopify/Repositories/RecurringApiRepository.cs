using System;
using System.Net;
using Autofac.Extras.DynamicProxy2;
using Newtonsoft.Json;
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
    public class RecurringApiRepository : IRecurringApiRepository
    {
        private readonly IHttpClientFacade _client;
        private readonly ShopifyRequestFactory _requestFactory;
        private readonly IPushLogger _logger;

        public ShopifyCredentials ShopifyCredentials { get; set; }


        public RecurringApiRepository(
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

        
        public virtual RecurringApplicationCharge UpsertCharge(RecurringApplicationCharge input)
        {
            var path = "/admin/api/2019-10/recurring_application_charges.json";
            var json = new { recurring_application_charge = input }.SerializeToJson();

            var request = _requestFactory.HttpPost(ShopifyCredentials, path, json);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            
            return RecurringApplicationCharge.FromDynamic(parent.recurring_application_charge);
        }


        public virtual ApplicationCreditParent
                    CreateApplicationCredit(decimal amount, string description, bool test)
        {
            var json = new
            {
                application_credit = new
                {
                    description = description,
                    amount = amount,
                    test = test
                }
            }.SerializeToJson();

            var path = "/admin/api/2019-10/application_credits.json";
            var request = 
                _requestFactory.HttpPost(
                    ShopifyCredentials, path, json);
            var response = _client.ExecuteRequest(request);

            return response
                    .Body
                    .DeserializeFromJson<ApplicationCreditParent>();
        }

        [Obsolete]
        public virtual RecurringApplicationCharge UpdateChargeAmount(long id, decimal amount)
        {
            var path = 
                $"/admin/api/2019-10/recurring_application_charges/{id}/customize.json" +
                $"?recurring_application_charge[capped_amount]={amount}";

            var request = _requestFactory.HttpPut(ShopifyCredentials, path, "");
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);

            return RecurringApplicationCharge.FromDynamic(parent.recurring_application_charge);
        }

        public virtual RecurringApplicationCharge RetrieveCharge(long id)
        {
            var path = $"/admin/api/2019-10/recurring_application_charges/{id}.json";
            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);
            
            _logger.Trace($"Status Code: {clientResponse.StatusCode}");
            _logger.Trace(clientResponse.Body);

            if (clientResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);

            return RecurringApplicationCharge.FromDynamic(parent.recurring_application_charge);
        }

        public virtual RecurringApplicationCharge ActivateCharge(RecurringApplicationCharge input)
        {
            var path = $"/admin/api/2019-10/recurring_application_charges/{input.id}/activate.json";
            var json = new { recurring_application_charge = input }.SerializeToJson();

            var request = _requestFactory.HttpPost(ShopifyCredentials, path, json);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);

            return RecurringApplicationCharge.FromDynamic(parent.recurring_application_charge);
        }
        

        public virtual void CancelCharge(long id)
        {
            var path = $"/admin/api/2019-10/recurring_application_charges/{id}.json";
            var request = _requestFactory.HttpDelete(ShopifyCredentials, path);
            _client.ExecuteRequest(request);
        }
    }
}

