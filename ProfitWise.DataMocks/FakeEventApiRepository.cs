using System;
using System.Collections.Generic;
using Autofac.Extras.DynamicProxy2;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Model;

namespace ProfitWise.DataMocks
{
    [Intercept(typeof(ShopifyCredentialRequired))]
    public class FakeEventApiRepository : IEventApiRepository
    {
        public ShopifyCredentials ShopifyCredentials { get; set; }
        public int RetrieveCount(EventFilter filter)
        {
            throw new NotImplementedException();
        }

        public IList<Event> Retrieve(EventFilter filter, int page = 1, int limit = 250)
        {
            throw new NotImplementedException();
        }
    }
}
