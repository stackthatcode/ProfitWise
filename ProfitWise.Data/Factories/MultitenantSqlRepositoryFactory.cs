using System;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;

namespace ProfitWise.Data.Factories
{
    public class MultitenantSqlRepositoryFactory
    {
        private readonly Func<ShopifyProductRepository> _productRepositoryFactory;
        private readonly Func<ShopifyVariantRepository> _variantRepositoryFactory;
        private readonly Func<ShopifyOrderRepository> _orderRepositoryFactory;


        public MultitenantSqlRepositoryFactory(
            Func<ShopifyProductRepository> productRepositoryFactory,
            Func<ShopifyVariantRepository> variantRepositoryFactory, 
            Func<ShopifyOrderRepository> orderRepositoryFactory)
        {
            _productRepositoryFactory = productRepositoryFactory;
            _variantRepositoryFactory = variantRepositoryFactory;
            _orderRepositoryFactory = orderRepositoryFactory;
        }

        public virtual ShopifyProductRepository MakeProductRepository(ShopifyShop shop)
        {
            var repository = _productRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }

        public virtual ShopifyVariantRepository MakeVariantRepository(ShopifyShop shop)
        {
            var repository = _variantRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }

        public virtual ShopifyOrderRepository MakeOrderRepository(ShopifyShop shop)
        {
            var repository = _orderRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }
    }
}

