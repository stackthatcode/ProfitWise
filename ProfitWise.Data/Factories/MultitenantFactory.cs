using System;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories.Multitenant;
using ProfitWise.Data.Services;


namespace ProfitWise.Data.Factories
{
    public class MultitenantFactory
    {
        private readonly Func<ShopifyOrderRepository> _orderRepositoryFactory;
        private readonly Func<BatchStateRepository> _profitWiseBatchStateRepositoryFactory;
        private readonly Func<ProductRepository> _productRepositoryFactory;
        private readonly Func<VariantRepository> _variantRepositoryFactory;
        private readonly Func<CatalogBuilderService> _productVariantServiceFactory;
        private readonly Func<CatalogRetrievalService> _catalogRetrievalServiceFactory;
        private readonly Func<CogsEntryRepository> _cogsEntryRepositoryFactory;
        private readonly Func<CogsDownstreamRepository> _cogsDataUpdateRepositoryFactory;
        private readonly Func<PickListRepository> _pickListRepositoryFactory;
        private readonly Func<ReportRepository> _reportRepositoryFactory;
        private readonly Func<ReportFilterRepository> _filterRepositoryFactory;
        private readonly Func<ProfitRepository> _reportQueryRepositoryFactory;
        private readonly Func<CogsService> _orderCogsUpdateServiceFactory;
        private readonly Func<ConsolidationService> _consolidationServiceFactory;
        private readonly Func<GoodsOnHandRepository> _goodsOnHandRepositoryFactory;
        private readonly Func<DataService> _dataServiceFactory;
        private readonly Func<BillingRepository> _billingRepositoryFactory;
        private readonly Func<TourRepository> _tourRepositoryFactory;
        private readonly Func<UploadRepository> _uploadRepositoryFactory;

        public MultitenantFactory(
                Func<ShopifyOrderRepository> orderRepositoryFactory,
                Func<BatchStateRepository> profitWiseBatchStateRepositoryFactory,
                Func<ProductRepository> productRepositoryFactory,
                Func<VariantRepository> variantRepositoryFactory,
                Func<CatalogBuilderService> productVariantServiceFactory,
                Func<CatalogRetrievalService> catalogRetrievalServiceFactory,
                Func<CogsEntryRepository> cogsEntryRepositoryFactory, 
                Func<PickListRepository> pickListRepositoryFactory,
                Func<ReportRepository> reportRepositoryFactory, 
                Func<ReportFilterRepository> filterRepositoryFactory, 
                Func<ProfitRepository> reportQueryRepositoryFactory,
                Func<CogsService> orderCogsUpdateServiceFactory, 
                Func<CogsDownstreamRepository> cogsDataUpdateRepositoryFactory, 
                Func<ConsolidationService> consolidationServiceFactory, 
                Func<GoodsOnHandRepository> goodsOnHandRepositoryFactory, 
                Func<DataService> dataServiceFactory, 
                Func<BillingRepository> billingRepositoryFactory,
                Func<TourRepository> tourRepositoryFactory, 
                Func<UploadRepository> uploadRepositoryFactory)
        {
            _orderRepositoryFactory = orderRepositoryFactory;
            _profitWiseBatchStateRepositoryFactory = profitWiseBatchStateRepositoryFactory;
            _productRepositoryFactory = productRepositoryFactory;
            _variantRepositoryFactory = variantRepositoryFactory;
            _productVariantServiceFactory = productVariantServiceFactory;
            _catalogRetrievalServiceFactory = catalogRetrievalServiceFactory;
            _cogsEntryRepositoryFactory = cogsEntryRepositoryFactory;
            _pickListRepositoryFactory = pickListRepositoryFactory;
            _reportRepositoryFactory = reportRepositoryFactory;
            _filterRepositoryFactory = filterRepositoryFactory;
            _reportQueryRepositoryFactory = reportQueryRepositoryFactory;
            _orderCogsUpdateServiceFactory = orderCogsUpdateServiceFactory;
            _cogsDataUpdateRepositoryFactory = cogsDataUpdateRepositoryFactory;
            _consolidationServiceFactory = consolidationServiceFactory;
            _goodsOnHandRepositoryFactory = goodsOnHandRepositoryFactory;
            _dataServiceFactory = dataServiceFactory;
            _billingRepositoryFactory = billingRepositoryFactory;
            _tourRepositoryFactory = tourRepositoryFactory;
            _uploadRepositoryFactory = uploadRepositoryFactory;
        }

        public virtual BatchStateRepository MakeBatchStateRepository(PwShop shop)
        {
            var repository = _profitWiseBatchStateRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual BillingRepository MakeBillingRepository(PwShop shop)
        {
            var repository = _billingRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }
        
        public virtual CatalogBuilderService MakeCatalogBuilderService(PwShop shop)
        {
            var service = _productVariantServiceFactory();
            service.PwShop = shop;
            return service;
        }

        public virtual CatalogRetrievalService MakeCatalogRetrievalService(PwShop shop)
        {
            var service = _catalogRetrievalServiceFactory();
            service.PwShop = shop;
            return service;
        }

        public virtual CogsDownstreamRepository MakeCogsDownstreamRepository(PwShop shop)
        {
            var repository = _cogsDataUpdateRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }
        
        public virtual CogsEntryRepository MakeCogsEntryRepository(PwShop shop)
        {
            var repository = _cogsEntryRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual CogsService MakeCogsService(PwShop shop)
        {
            var repository = _orderCogsUpdateServiceFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual ConsolidationService MakeConsolidationService(PwShop shop)
        {
            var service = _consolidationServiceFactory();
            service.PwShop = shop;
            return service;
        }

        public virtual DataService MakeDataService(PwShop shop)
        {
            var service = _dataServiceFactory();
            service.PwShop = shop;
            return service;
        }

        public virtual GoodsOnHandRepository MakeGoodsOnHandRepository(PwShop shop)
        {
            var repository = _goodsOnHandRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual PickListRepository MakePickListRepository(PwShop shop)
        {
            var repository = _pickListRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual ProfitRepository MakeProfitRepository(PwShop shop)
        {
            var repository = _reportQueryRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual ProductRepository MakeProductRepository(PwShop shop)
        {
            var repository = _productRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual ReportRepository MakeReportRepository(PwShop shop)
        {
            var repository = _reportRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual ReportFilterRepository MakeReportFilterRepository(PwShop shop)
        {
            var repository = _filterRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual ShopifyOrderRepository MakeShopifyOrderRepository(PwShop shop)
        {
            var repository = _orderRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual TourRepository MakeTourRepository(PwShop shop)
        {
            var repository = _tourRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual VariantRepository MakeVariantRepository(PwShop shop)
        {
            var repository = _variantRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }

        public virtual UploadRepository MakeUploadRepository(PwShop shop)
        {
            var repository = _uploadRepositoryFactory();
            repository.PwShop = shop;
            return repository;
        }
    }
}

