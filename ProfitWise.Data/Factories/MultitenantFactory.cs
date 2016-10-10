using System;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;

namespace ProfitWise.Data.Factories
{
    public class MultitenantFactory
    {
        private readonly Func<ShopifyOrderRepository> _orderRepositoryFactory;
        private readonly Func<PwBatchStateRepository> _profitWiseBatchStateRepositoryFactory;
        private readonly Func<PwProductRepository> _productRepositoryFactory;
        private readonly Func<PwVariantRepository> _variantRepositoryFactory;
        private readonly Func<CatalogBuilderService> _productVariantServiceFactory;
        private readonly Func<PwCogsRepository> _cogsRepositoryFactory;
        private readonly Func<PwPickListRepository> _pickListRepositoryFactory;
        private readonly Func<PwFilterRepository> _filterRepositoryFactory;

        public MultitenantFactory(
            Func<ShopifyOrderRepository> orderRepositoryFactory,
            Func<PwBatchStateRepository> profitWiseBatchStateRepositoryFactory,
            Func<PwProductRepository> productRepositoryFactory,
            Func<PwVariantRepository> variantRepositoryFactory,
            Func<CatalogBuilderService> productVariantServiceFactory,
            Func<PwCogsRepository> cogsRepositoryFactory, 
            Func<PwPickListRepository> pickListRepositoryFactory,
            Func<PwFilterRepository> filterRepositoryFactory)
        {
            _orderRepositoryFactory = orderRepositoryFactory;
            _profitWiseBatchStateRepositoryFactory = profitWiseBatchStateRepositoryFactory;
            _productRepositoryFactory = productRepositoryFactory;
            _variantRepositoryFactory = variantRepositoryFactory;
            _productVariantServiceFactory = productVariantServiceFactory;
            _cogsRepositoryFactory = cogsRepositoryFactory;
            _pickListRepositoryFactory = pickListRepositoryFactory;
            _filterRepositoryFactory = filterRepositoryFactory;
        }

        public virtual ShopifyOrderRepository MakeShopifyOrderRepository(PwShop shop)
        {
            var repository = _orderRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }
        
        public virtual PwBatchStateRepository MakeBatchStateRepository(PwShop shop)
        {
            var repository = _profitWiseBatchStateRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual PwProductRepository MakeProductRepository(PwShop shop)
        {
            var repository = _productRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }
        public virtual PwVariantRepository MakeVariantRepository(PwShop shop)
        {
            var repository = _variantRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual CatalogBuilderService MakeCatalogBuilderService(PwShop shop)
        {
            var service = _productVariantServiceFactory();
            service.PwShop = shop;
            return service;
        }

        public virtual PwCogsRepository MakeCogsRepository(PwShop shop)
        {
            var repository = _cogsRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual PwPickListRepository MakePickListRepository(PwShop shop)
        {
            var repository = _pickListRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual PwFilterRepository MakeFilterRepository(PwShop shop)
        {
            var repository = _filterRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }
    }
}

