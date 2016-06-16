using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProfitWise.Batch.Factory;
using ProfitWise.Batch.Orders;
using ProfitWise.Batch.Products;
using Push.Utilities.Logging;

namespace ProfitWise.Batch
{
    public interface IMasterProcess
    {
        void Execute(string userId);
    }


    public class MasterProcess : IMasterProcess
    {
        private readonly ILogger _logger;
        private readonly int _refreshServiceShopifyOrderLimit;

        public MasterProcess(ILogger logger)
        {
            _refreshServiceShopifyOrderLimit =
                Int32.Parse(ConfigurationManager.AppSettings["RefreshServiceShopifyOrderLimit"]);

            _logger = logger;
        }

        public void Execute(string userId)
        {
            var shopifyClientFactory = new ShopifyHttpClientFactory(_logger);
            //var shopifyClientFactory = new ShopifyNaughtyClientFactory(_logger);

            var shopifyHttpClient = shopifyClientFactory.Make(userId);
            shopifyHttpClient.ShopifyRetriesEnabled = true;
            shopifyHttpClient.ThrowExceptionOnBadHttpStatusCode = true;

            var productRefreshService = new ProductRefreshService(userId, _logger, shopifyHttpClient);
            productRefreshService.Execute();

            var orderRefreshService = new OrderRefreshService(userId, _logger, shopifyHttpClient);
            orderRefreshService.ShopifyOrderLimit = _refreshServiceShopifyOrderLimit;
            orderRefreshService.Execute();
        }
    }
}
