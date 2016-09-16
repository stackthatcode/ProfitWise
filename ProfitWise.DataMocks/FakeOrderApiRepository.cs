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
    public class FakeOrderApiRepository : IOrderApiRepository
    {
        public Shop Retrieve()
        {
            throw new NotImplementedException();
        }

        public ShopifyCredentials ShopifyCredentials { get; set; }
        public int RetrieveCount(OrderFilter filter)
        {
            throw new NotImplementedException();
        }

        public IList<Order> Retrieve(OrderFilter filter, int page = 1, int limit = 250)
        {
            throw new NotImplementedException();
        }

        //public Order 
    }
}
