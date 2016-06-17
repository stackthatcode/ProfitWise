using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProfitWise.Batch.Orders;
using ProfitWise.Batch.Products;
using Push.Shopify.HttpClient;
using Push.Utilities.Logging;
using Push.Utilities.Web.Identity;

namespace ProfitWise.Batch
{
    public interface IMasterProcess
    {
        void Execute(string userId);
    }


    public class MasterProcess : IMasterProcess
    {
        private readonly ShopifyCredentialService _shopifyCredentialService;
        private readonly OrderRefreshService _orderRefreshService;
        private readonly ProductRefreshService _productRefreshService;
        private readonly ILogger _logger;

        public MasterProcess(
                ShopifyCredentialService shopifyCredentialService,
                OrderRefreshService orderRefreshService,
                ProductRefreshService productRefreshService,
                ILogger logger)
        {
            // TODO: move into Autofac configuration
           
            _shopifyCredentialService = shopifyCredentialService;
            _orderRefreshService = orderRefreshService;
            _productRefreshService = productRefreshService;
            _logger = logger;
        }

        public void Execute(string userId)
        {
            var shopifyFromClaims = _shopifyCredentialService.Retrieve(userId);

            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(
                    $"ShopifyCredentialService unable to Retrieve for User {userId}: {shopifyFromClaims.Message}");
            }

            var shopifyClientCredentials = new ShopifyCredentials()
            {
                ShopOwnerId = shopifyFromClaims.ShopOwnerUserId,
                ShopDomain = shopifyFromClaims.ShopDomain,
                AccessToken = shopifyFromClaims.AccessToken,
            };

            _productRefreshService.Execute(shopifyClientCredentials);
            _orderRefreshService.Execute(shopifyClientCredentials);
        }
    }
}
