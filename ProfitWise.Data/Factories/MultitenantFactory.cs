﻿using System;
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


        public MultitenantFactory(
            Func<ShopifyOrderRepository> orderRepositoryFactory,
            Func<PwBatchStateRepository> profitWiseBatchStateRepositoryFactory,
            Func<PwProductRepository> productRepositoryFactory,
            Func<PwVariantRepository> variantRepositoryFactory,
            Func<CatalogBuilderService> productVariantServiceFactory)
        {
            _orderRepositoryFactory = orderRepositoryFactory;
            _profitWiseBatchStateRepositoryFactory = profitWiseBatchStateRepositoryFactory;
            _productRepositoryFactory = productRepositoryFactory;
            _variantRepositoryFactory = variantRepositoryFactory;
            _productVariantServiceFactory = productVariantServiceFactory;
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
    }
}
