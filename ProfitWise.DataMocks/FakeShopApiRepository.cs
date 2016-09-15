using System;
using System.Linq;
using System.Text;
using Autofac.Extras.DynamicProxy2;
using Castle.Core.Internal;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Model;

namespace ProfitWise.DataMocks
{
    [Intercept(typeof(ShopifyCredentialRequired))]
    public class FakeShopApiRepository : IShopApiRepository
    {
        public Shop Retrieve()
        {
            var bytes = Encoding.ASCII.GetBytes(ShopifyCredentials.ShopOwnerUserId);
            long fakeId = 0;

            for (var counter = 0; counter < bytes.Length; counter++)
            {
                fakeId += (long)Math.Pow(10, counter) * bytes[counter];
            }

            return new Shop()
            {
                Currency = "USD",
                Id = fakeId,
            };
        }


        public ShopifyCredentials ShopifyCredentials { get; set; }
    }
}
