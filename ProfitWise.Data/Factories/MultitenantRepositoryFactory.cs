using System;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;

namespace ProfitWise.Data.Factories
{
    public class MultitenantRepositoryFactory
    {
        private readonly Func<ShopifyOrderRepository> _orderRepositoryFactory;
        private readonly Func<PwBatchStateRepository> _profitWiseBatchStateRepositoryFactory;
        private readonly Func<PwProductRepository> _productRepositoryFactory;
        private readonly Func<PwVariantRepository> _variantRepositoryFactory;
        private readonly Func<PwPreferencesRepository> _preferencesRepositoryFactory;


        public MultitenantRepositoryFactory(
            Func<ShopifyOrderRepository> orderRepositoryFactory,
            Func<PwBatchStateRepository> profitWiseBatchStateRepositoryFactory,
            Func<PwProductRepository> productRepositoryFactory,
            Func<PwVariantRepository> variantRepositoryFactory,
            Func<PwPreferencesRepository> preferencesRepositoryFactory 
            )
        {
            _orderRepositoryFactory = orderRepositoryFactory;
            _profitWiseBatchStateRepositoryFactory = profitWiseBatchStateRepositoryFactory;
            _productRepositoryFactory = productRepositoryFactory;
            _variantRepositoryFactory = variantRepositoryFactory;
            _preferencesRepositoryFactory = preferencesRepositoryFactory;
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
            var repository = _productRepositoryFactory();
            repository.PwShopId = shop.ShopId;
            return repository;
        }
        public virtual PwVariantRepository MakeVariantRepository(PwShop shop)
        {
            var repository = _variantRepositoryFactory();
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

