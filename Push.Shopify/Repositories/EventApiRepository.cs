using System.Collections.Generic;
using Autofac.Extras.DynamicProxy2;
using Newtonsoft.Json;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace Push.Shopify.Repositories
{
    [Intercept(typeof(ShopifyCredentialRequired))]
    public class EventApiRepository : IShopifyCredentialConsumer
    {
        private readonly IShopifyHttpClient _client;
        private readonly ShopifyRequestFactory _requestFactory;
        public ShopifyCredentials ShopifyCredentials { get; set; }

        public EventApiRepository(
                IShopifyHttpClient client,
                ShopifyRequestFactory requestFactory)
        {
            _client = client;
            _requestFactory = requestFactory;
        }
        
        public virtual int RetrieveCount(EventFilter filter)
        {
            var url = "/admin/events/count.json?" + filter.ToQueryStringBuilder();
            var request = _requestFactory.HttpGet(ShopifyCredentials, url);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }


        public virtual IList<Event> Retrieve(EventFilter filter, int page = 1, int limit = 250)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("page", page)
                    .Add("limit", limit)
                    .Add(filter.ToQueryStringBuilder())
                    .ToString();

            var url = "/admin/events.json?" + querystring;
            var request = _requestFactory.HttpGet(ShopifyCredentials, url);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);

            var output = new List<Event>();
            foreach (var @event in parent.events)
            {
                var result = new Event();
                result.Id = @event.id;
                result.SubjectId = @event.subject_id;
                result.SubjectType = @event.subject_type;
                result.Verb = @event.verb;
                output.Add(result);
            }
            return output;
        }
    }
}

