using System;
using MySql.Data.MySqlClient;
using ProfitWise.Batch.Factory;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;
using Push.Utilities.Logging;

namespace ProfitWise.Batch.Products
{
    public class ProductRefreshService : IDisposable
    {
        private readonly string _userId;

        private readonly ProductApiRepository _productApiRepository;
        private readonly ProductDataRepository _productDataRepository;
        private readonly VariantDataRepository _variantDataRepository;
        private readonly MySqlConnection _connection;

        public ProductRefreshService(string userId, ILogger logger, IShopifyClientFactory shopifyClientFactory)
        {
            _userId = userId;

            var shopifyClient = shopifyClientFactory.Make(userId);
            _productApiRepository = new ProductApiRepository(shopifyClient, logger);

            _connection = MySqlConnectionFactory.Make();
            _productDataRepository = new ProductDataRepository(_userId, _connection);
            _variantDataRepository = new VariantDataRepository(_userId, _connection);
        }

        public void Execute()
        {
            var allproducts =
                _productApiRepository.RetrieveAll(
                    BatchConfiguration.ShopifyPageSize, BatchConfiguration.ShopifyRequestPause);

            foreach (var product in allproducts)
            {
                var productData = new ProductData()
                {
                    UserId = _userId,
                    ShopifyProductId = product.Id,
                    Title = product.Title,
                };

                _productDataRepository.Delete(product.Id);
                _variantDataRepository.DeleteByProduct(product.Id);

                _productDataRepository.Insert(productData);

                foreach (var variant in product.Variants)
                {
                    var variantData = new VariantData()
                    {
                        UserId = _userId,
                        ShopifyVariantId = variant.Id,
                        ShopifyProductId = product.Id,
                        Price = variant.Price,
                        Sku = variant.Sku,
                        Title = variant.Title,
                    };

                    _variantDataRepository.Insert(variantData);
                };
            }
        }


        public void Dispose()
        {
            _connection.Close();
        }
    }
}
