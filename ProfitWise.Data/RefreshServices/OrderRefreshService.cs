using System.Collections.Generic;
using System.Linq;
using ProfitWise.Batch.RefreshServices;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.General;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.RefreshServices
{
    public class OrderRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantRepositoryFactory _multitenantRepositoryFactory;
        private readonly RefreshServiceConfiguration _refreshServiceConfiguration;
        private readonly ShopRepository _shopRepository;


        public OrderRefreshService(
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantRepositoryFactory multitenantRepositoryFactory,
                RefreshServiceConfiguration refreshServiceConfiguration,
                ShopRepository shopRepository,
                IPushLogger logger)

        {
            _pushLogger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantRepositoryFactory = multitenantRepositoryFactory;
            _refreshServiceConfiguration = refreshServiceConfiguration;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(OrderFilter filter, ShopifyCredentials shopCredentials)
        {
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerId);
            var orderApiRepository = _apiRepositoryFactory.MakeOrderApiRepository(shopCredentials);

            var count = orderApiRepository.RetrieveCount(filter);
            _pushLogger.Info($"{this.ClassAndMethodName()} - {count} Orders to process");

            var numberofpages = 
                PagingFunctions.NumberOfPages(
                    _refreshServiceConfiguration.MaxOrderRate, count);

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info(
                    $"{this.ClassAndMethodName()} - page {pagenumber} of {numberofpages} pages");
                
                var orders = orderApiRepository.Retrieve(filter, pagenumber, _refreshServiceConfiguration.MaxOrderRate);

                WriteOrdersToPersistence(shop, orders);
            }
        }


        protected virtual void WriteOrdersToPersistence(ShopifyShop shop, IList<Order> results)
        {
            var orderRepository = _multitenantRepositoryFactory.MakeOrderRepository(shop);

            _pushLogger.Info($"{this.ClassAndMethodName()} - {results.Count} Orders to process");

            using (var trans = orderRepository.InitiateTransaction())
            {
                var shopifyOrders = results.Select(x => x.ToShopifyOrder(shop.ShopId)).ToList();

                orderRepository.MassDelete(shopifyOrders);

                foreach (var order in shopifyOrders)
                {
                    _pushLogger.Debug($"Refresh Order: {order.OrderNumber} / {order.ShopifyOrderId} for {order.Email}");

                    orderRepository.InsertOrder(order);

                    foreach (var line_item in order.LineItems)
                    {
                        orderRepository.InsertOrderLineItem(line_item);
                    }
                }

                trans.Commit();
            }
        }

    }
}

