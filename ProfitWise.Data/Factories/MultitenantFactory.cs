using System;
using ProfitWise.Data.Model.Shop;
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
        private readonly Func<PwReportRepository> _reportRepositoryFactory;
        private readonly Func<PwReportFilterRepository> _filterRepositoryFactory;
        private readonly Func<PwReportQueryRepository> _reportQueryRepositoryFactory;
        private readonly Func<CogsUpdateService> _orderCogsUpdateServiceFactory;

        public MultitenantFactory(
            Func<ShopifyOrderRepository> orderRepositoryFactory,
            Func<PwBatchStateRepository> profitWiseBatchStateRepositoryFactory,
            Func<PwProductRepository> productRepositoryFactory,
            Func<PwVariantRepository> variantRepositoryFactory,
            Func<CatalogBuilderService> productVariantServiceFactory,
            Func<PwCogsRepository> cogsRepositoryFactory, 
            Func<PwPickListRepository> pickListRepositoryFactory,
            Func<PwReportRepository> reportRepositoryFactory, 
            Func<PwReportFilterRepository> filterRepositoryFactory, 
            Func<PwReportQueryRepository> reportQueryRepositoryFactory,
            Func<CogsUpdateService> orderCogsUpdateServiceFactory
            )
        {
            _orderRepositoryFactory = orderRepositoryFactory;
            _profitWiseBatchStateRepositoryFactory = profitWiseBatchStateRepositoryFactory;
            _productRepositoryFactory = productRepositoryFactory;
            _variantRepositoryFactory = variantRepositoryFactory;
            _productVariantServiceFactory = productVariantServiceFactory;
            _cogsRepositoryFactory = cogsRepositoryFactory;
            _pickListRepositoryFactory = pickListRepositoryFactory;
            _reportRepositoryFactory = reportRepositoryFactory;
            _filterRepositoryFactory = filterRepositoryFactory;
            _reportQueryRepositoryFactory = reportQueryRepositoryFactory;
            _orderCogsUpdateServiceFactory = orderCogsUpdateServiceFactory;
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
        
        public virtual PwReportRepository MakeReportRepository(PwShop shop)
        {
            var repository = _reportRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual PwReportFilterRepository MakeReportFilterRepository(PwShop shop)
        {
            var repository = _filterRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual PwReportQueryRepository MakeReportQueryRepository(PwShop shop)
        {
            var repository = _reportQueryRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual CogsUpdateService MakeCogsUpdateService(PwShop shop)
        {
            var repository = _orderCogsUpdateServiceFactory();
            repository.PwShop = shop;
            return repository;
        }
    }
}

