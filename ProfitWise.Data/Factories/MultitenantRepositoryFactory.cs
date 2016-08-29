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

        public virtual ShopifyProductRepository MakeShopifyProductRepository(PwShop shop)
        {
            var repository = _productRepositoryFactory();
            repository.PwShopId = shop.ShopId;
            return repository;
        }

        public virtual ShopifyVariantRepository MakeShopifyVariantRepository(PwShop shop)
        {
            var repository = _variantRepositoryFactory();
            repository.PwShopId = shop.ShopId;
            return repository;
        }

        public virtual ShopifyOrderRepository MakeShopifyOrderRepository(PwShop shop)
        {
            var repository = _orderRepositoryFactory();
            repository.PwShopId = shop.ShopId;
            return repository;
        }
        
        public virtual PwBatchStateRepository MakeBatchStateRepository(PwShop shop)
        {
            var repository = _profitWiseBatchStateRepositoryFactory();
            repository.PwShopId = shop.ShopId;
            return repository;
        }

        public virtual PwProductRepository MakeProductRepository(PwShop shop)
        {
            var repository = _profitWiseProductRepositoryFactory();
            repository.PwShopId = shop.ShopId;
            return repository;
        }

        public virtual PwPreferencesRepository MakePreferencesRepository(PwShop shop)
        {
            var repository = _preferencesRepositoryFactory();
            repository.PwShopId = shop.ShopId;
            return repository;
        }

    }
}

