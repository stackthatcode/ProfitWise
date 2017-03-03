using System;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;

namespace Push.Shopify.Factories
{
    public class ApiRepositoryFactory
    {
        private readonly Func<IOrderApiRepository> _orderApiRepositoryFactory;
        private readonly Func<IProductApiRepository> _productApiRepositoryFactory;
        private readonly Func<IEventApiRepository> _eventApiRepositoryFactory;
        private readonly Func<IShopApiRepository> _shopApiRepositoryFactory;
        private readonly Func<IRecurringApiRepository> _recurringApiRepositoryFactory;


        public ApiRepositoryFactory(
            Func<IOrderApiRepository> orderApiRepositoryFactory,
            Func<IProductApiRepository> productApiRepositoryFactory,
            Func<IEventApiRepository> eventApiRepositoryFactory,
            Func<IShopApiRepository> shopApiRepositoryFactory, 
            Func<IRecurringApiRepository> recurringApiRepositoryFactory)
        {
            _orderApiRepositoryFactory = orderApiRepositoryFactory;
            _productApiRepositoryFactory = productApiRepositoryFactory;
            _eventApiRepositoryFactory = eventApiRepositoryFactory;
            _shopApiRepositoryFactory = shopApiRepositoryFactory;
            _recurringApiRepositoryFactory = recurringApiRepositoryFactory;
        }

        public virtual IOrderApiRepository MakeOrderApiRepository(ShopifyCredentials credentials)
        {
            var repository = _orderApiRepositoryFactory();
            repository.ShopifyCredentials = credentials;
            return repository;
        }

        public virtual IProductApiRepository MakeProductApiRepository(ShopifyCredentials credentials)
        {
            var repository = _productApiRepositoryFactory();
            repository.ShopifyCredentials = credentials;
            return repository;
        }

        public virtual IEventApiRepository MakeEventApiRepository(ShopifyCredentials credentials)
        {
            var repository = _eventApiRepositoryFactory();
            repository.ShopifyCredentials = credentials;
            return repository;
        }

        public virtual IShopApiRepository MakeShopApiRepository(ShopifyCredentials shopifyCredentials)
        {
            var repository = _shopApiRepositoryFactory();
            repository.ShopifyCredentials = shopifyCredentials;
            return repository;
        }

        public virtual IRecurringApiRepository MakeRecurringApiRepository(ShopifyCredentials shopifyCredentials)
        {
            var repository = _recurringApiRepositoryFactory();
            repository.ShopifyCredentials = shopifyCredentials;
            return repository;
        }
    }
}

