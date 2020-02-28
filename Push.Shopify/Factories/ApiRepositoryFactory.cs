using System;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Repositories;

namespace Push.Shopify.Factories
{
    public class ApiRepositoryFactory
    {
        private readonly Func<OrderApiRepository> _orderApiRepositoryFactory;
        private readonly Func<ProductApiRepository> _productApiRepositoryFactory;
        private readonly Func<IEventApiRepository> _eventApiRepositoryFactory;
        private readonly Func<IShopApiRepository> _shopApiRepositoryFactory;
        private readonly Func<IRecurringApiRepository> _recurringApiRepositoryFactory;
        private readonly Func<IWebhookApiRepository> _webhookApiRepositoryFactory;
        private readonly Func<OAuthRepository> _oAuthRepositoryFactory;


        public ApiRepositoryFactory(
            Func<OrderApiRepository> orderApiRepositoryFactory,
            Func<ProductApiRepository> productApiRepositoryFactory,
            Func<IEventApiRepository> eventApiRepositoryFactory,
            Func<IShopApiRepository> shopApiRepositoryFactory, 
            Func<IRecurringApiRepository> recurringApiRepositoryFactory, 
            Func<IWebhookApiRepository> webhookApiRepositoryFactory, 
            Func<OAuthRepository> oAuthRepositoryFactory)
        {
            _orderApiRepositoryFactory = orderApiRepositoryFactory;
            _productApiRepositoryFactory = productApiRepositoryFactory;
            _eventApiRepositoryFactory = eventApiRepositoryFactory;
            _shopApiRepositoryFactory = shopApiRepositoryFactory;
            _recurringApiRepositoryFactory = recurringApiRepositoryFactory;
            _webhookApiRepositoryFactory = webhookApiRepositoryFactory;
            _oAuthRepositoryFactory = oAuthRepositoryFactory;
        }

        public virtual IWebhookApiRepository 
                MakeWebhookApiRepository(ShopifyCredentials credentials)
        {
            var repository = _webhookApiRepositoryFactory();
            repository.ShopifyCredentials = credentials;
            return repository;
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

        public virtual OAuthRepository MakeOAuthRepository(ShopifyCredentials credentials)
        {
            var repository = _oAuthRepositoryFactory();
            repository.ShopifyCredentials = credentials;
            return repository;           
        }
    }
}

