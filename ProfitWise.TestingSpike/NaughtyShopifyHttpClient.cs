using System;
using System.Configuration;
using System.Net;
using Microsoft.AspNet.Identity.EntityFramework;
using ProfitWise.Batch.Factory;
using Push.Shopify.HttpClient;
using Push.Utilities.Web.Identity;

namespace ProfitWise.Batch
{
    public class ShopifyNaughtyClientFactory : IShopifyClientFactory
    {
        public IShopifyHttpClient Make(string userId)
        {
            var context = ApplicationDbContext.Create();
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            var shopifyCredentialService = new ShopifyCredentialService(userManager);
            var shopifyFromClaims = shopifyCredentialService.Retrieve(userId);

            // TODO - notify the Alerting Service on this type of failure

            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(shopifyFromClaims.Message);
            }

            var httpClient = new NaughtyHttpClient();
            var shopifyClient =
                new ShopifyHttpClient(httpClient, shopifyFromClaims.ShopDomain, shopifyFromClaims.AccessToken);

            return shopifyClient;
        }
    }

    public class NaughtyHttpClient : IHttpClient
    {
        private IHttpClient _httpClient;
        Random rnd = new Random();

        public NaughtyHttpClient()
        {
            _httpClient = new HttpClient();
        }        

        public HttpClientResponse ProcessRequest(HttpWebRequest request)
        {
            int pick = rnd.Next(1, 10);

            if (pick > 5)
            {
                return new HttpClientResponse()
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                };
            }

            return _httpClient.ProcessRequest(request);
        }
    }

}
