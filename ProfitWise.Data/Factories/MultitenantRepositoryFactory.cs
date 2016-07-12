using System;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;

namespace ProfitWise.Data.Factories
{
    public class MultitenantRepositoryFactory
    {
        private readonly Func<ShopifyProductRepository> _productRepositoryFactory;
        private readonly Func<ShopifyVariantRepository> _variantRepositoryFactory;
        private readonly Func<ShopifyOrderRepository> _orderRepositoryFactory;
        private readonly Func<PwBatchStateRepository> _profitWiseBatchStateRepositoryFactory;
        private readonly Func<PwProductRepository> _profitWiseProductRepositoryFactory;
        private readonly Func<PwPreferencesRepository> _preferencesRepositoryFactory;


        public MultitenantRepositoryFactory(
            Func<ShopifyProductRepository> productRepositoryFactory,
            Func<ShopifyVariantRepository> variantRepositoryFactory, 
            Func<ShopifyOrderRepository> orderRepositoryFactory,
            Func<PwBatchStateRepository> profitWiseBatchStateRepositoryFactory,
            Func<PwProductRepository> profitWiseProductRepositoryFactory,
            Func<PwPreferencesRepository> preferencesRepositoryFactory 
            )
        {
            _productRepositoryFactory = productRepositoryFactory;
            _variantRepositoryFactory = variantRepositoryFactory;
            _orderRepositoryFactory = orderRepositoryFactory;
            _profitWiseBatchStateRepositoryFactory = profitWiseBatchStateRepositoryFactory;
            _profitWiseProductRepositoryFactory = profitWiseProductRepositoryFactory;
            _preferencesRepositoryFactory = preferencesRepositoryFactory;
        }

        public virtual ShopifyProductRepository MakeShopifyProductRepository(ShopifyShop shop)
        {
            var repository = _productRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }

        public virtual ShopifyVariantRepository MakeShopifyVariantRepository(ShopifyShop shop)
        {
            var repository = _variantRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }

        public virtual ShopifyOrderRepository MakeShopifyOrderRepository(ShopifyShop shop)
        {
            var repository = _orderRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }
        
        public virtual PwBatchStateRepository MakeBatchStateRepository(ShopifyShop shop)
        {
            var repository = _profitWiseBatchStateRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }

        public virtual PwProductRepository MakeProductRepository(ShopifyShop shop)
        {
            var repository = _profitWiseProductRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }

        public virtual PwPreferencesRepository MakePreferencesRepository(ShopifyShop shop)
        {
            var repository = _preferencesRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }

    }
}

