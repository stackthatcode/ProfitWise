using System;
using System.Configuration;
using System.Net;
using Microsoft.AspNet.Identity.EntityFramework;
using ProfitWise.Batch.Factory;
using Push.Shopify.HttpClient;
using Push.Utilities.Logging;
using Push.Utilities.Web.Identity;

namespace ProfitWise.Batch
{
    public class ShopifyNaughtyClientFactory : IShopifyClientFactory
    {
        private readonly ILogger _logger;

        public ShopifyNaughtyClientFactory(ILogger logger)
        {
            _logger = logger;
        }

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

            //var httpClient = new HttpClientThatThrowsErrors();
            var httpClient = new HttpClientThatThrowsUnauthorized();
            
            var shopifyClient = new ShopifyHttpClient(httpClient, _logger, shopifyFromClaims.ShopDomain, shopifyFromClaims.AccessToken);


            // Load configuration values
            if (ConfigurationManager.AppSettings["ShopifyRetryLimit"] != null)
                shopifyClient.ShopifyRetryLimit = Int32.Parse(ConfigurationManager.AppSettings["ShopifyRetryLimit"]);
            if (ConfigurationManager.AppSettings["ShopifyHttpTimeout"] != null)
                shopifyClient.ShopifyHttpTimeout = Int32.Parse(ConfigurationManager.AppSettings["ShopifyHttpTimeout"]);
            if (ConfigurationManager.AppSettings["ShopifyThrottlingDelay"] != null)
                shopifyClient.ShopifyThrottlingDelay = Int32.Parse(ConfigurationManager.AppSettings["ShopifyThrottlingDelay"]);

            return shopifyClient;
        }
    }

    public class HttpClientThatThrowsUnauthorized : IHttpClient
    {
        private IHttpClient _httpClient;
        Random rnd = new Random();

        public HttpClientThatThrowsUnauthorized()
        {
            _httpClient = new HttpClient();
        }        

        public HttpClientResponse ProcessRequest(HttpWebRequest request)
        {
            int pick = rnd.Next(1, 5);

            if (pick == 4)
            {
                return new HttpClientResponse()
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                };
            }

            return _httpClient.ProcessRequest(request);
        }
    }


    public class HttpClientThatThrowsErrors : IHttpClient
    {
        private IHttpClient _httpClient;
        Random rnd = new Random();

        public HttpClientThatThrowsErrors()
        {
            _httpClient = new HttpClient();
        }

        public HttpClientResponse ProcessRequest(HttpWebRequest request)
        {
            int pick = rnd.Next(1, 5);

            if (pick > 3)
            {
                throw new Exception("System fault - network crash");
            }

            return _httpClient.ProcessRequest(request);
        }
    }

}
