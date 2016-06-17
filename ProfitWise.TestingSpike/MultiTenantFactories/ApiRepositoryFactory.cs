using System;
using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;

namespace ProfitWise.Batch.MultiTenantFactories
{
    public class ApiRepositoryFactory
    {
        private readonly Func<OrderApiRepository> _orderApiRepositoryFactory;
        private readonly Func<ProductApiRepository> _productApiRepositoryFactory;


        public ApiRepositoryFactory(
            Func<OrderApiRepository> orderApiRepositoryFactory,
            Func<ProductApiRepository> productApiRepositoryFactory)
        {
            _orderApiRepositoryFactory = orderApiRepositoryFactory;
            _productApiRepositoryFactory = productApiRepositoryFactory;
        }

        public virtual OrderApiRepository MakeOrderApiRepository(ShopifyCredentials credentials)
        {
            var repository = _orderApiRepositoryFactory();
            repository.ShopifyCredentials = credentials;
            return repository;
        }

        public virtual ProductApiRepository MakeProductApiRepository(ShopifyCredentials credentials)
        {
            var repository = _productApiRepositoryFactory();
            repository.ShopifyCredentials = credentials;
            return repository;
        }
    }
}

