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
        private readonly Func<ProfitWiseBatchStateRepository> _profitWiseBatchStateRepositoryFactory;
        private readonly Func<ProfitWiseProductRepository> _profitWiseProductRepositoryFactory;


        public MultitenantRepositoryFactory(
            Func<ShopifyProductRepository> productRepositoryFactory,
            Func<ShopifyVariantRepository> variantRepositoryFactory, 
            Func<ShopifyOrderRepository> orderRepositoryFactory,
            Func<ProfitWiseBatchStateRepository> profitWiseBatchStateRepositoryFactory,
            Func<ProfitWiseProductRepository> profitWiseProductRepositoryFactory)
        {
            _productRepositoryFactory = productRepositoryFactory;
            _variantRepositoryFactory = variantRepositoryFactory;
            _orderRepositoryFactory = orderRepositoryFactory;
            _profitWiseBatchStateRepositoryFactory = profitWiseBatchStateRepositoryFactory;
            _profitWiseProductRepositoryFactory = profitWiseProductRepositoryFactory;

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
        
        public virtual ProfitWiseBatchStateRepository MakeProfitWiseBatchStateRepository(ShopifyShop shop)
        {
            var repository = _profitWiseBatchStateRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }

        public virtual ProfitWiseProductRepository MakeProfitWiseProductRepository(ShopifyShop shop)
        {
            var repository = _profitWiseProductRepositoryFactory();
            repository.ShopId = shop.ShopId;
            return repository;
        }
    }
}

