using System;
using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;

namespace Push.Shopify.Factories
{
    public class ApiRepositoryFactory
    {
        private readonly Func<OrderApiRepository> _orderApiRepositoryFactory;
        private readonly Func<ProductApiRepository> _productApiRepositoryFactory;
        private readonly Func<EventApiRepository> _eventApiRepositoryFactory;
        private readonly Func<ShopApiRepository> _shopApiRepositoryFactory;


        public ApiRepositoryFactory(
            Func<OrderApiRepository> orderApiRepositoryFactory,
            Func<ProductApiRepository> productApiRepositoryFactory,
            Func<EventApiRepository> eventApiRepositoryFactory,
            Func<ShopApiRepository> shopApiRepositoryFactory)
        {
            _orderApiRepositoryFactory = orderApiRepositoryFactory;
            _productApiRepositoryFactory = productApiRepositoryFactory;
            _eventApiRepositoryFactory = eventApiRepositoryFactory;
            _shopApiRepositoryFactory = shopApiRepositoryFactory;
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

        public virtual EventApiRepository MakeEventApiRepository(ShopifyCredentials credentials)
        {
            var repository = _eventApiRepositoryFactory();
            repository.ShopifyCredentials = credentials;
            return repository;
        }

        public virtual ShopApiRepository MakeShopApiRepository(ShopifyCredentials shopifyCredentials)
        {
            var repository = _shopApiRepositoryFactory();
            repository.ShopifyCredentials = shopifyCredentials;
            return repository;
        }
    }
}

