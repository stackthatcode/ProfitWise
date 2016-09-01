using System;
using System.Collections.Generic;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.ProcessSteps
{
    public class ProductCleanupStep
    {
        private readonly IPushLogger _pushLogger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly RefreshServiceConfiguration _configuration;
        private readonly PwShopRepository _shopRepository;


        public ProductCleanupStep(
                IPushLogger logger,
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantFactory multitenantFactory,
                RefreshServiceConfiguration configuration,
                PwShopRepository shopRepository)
        {
            _pushLogger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantFactory = multitenantFactory;
            _configuration = configuration;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            // Get Shopify Shop
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerId);

            // Create an instance of multi-tenant-aware repositories
            var productRepository = this._multitenantFactory.MakeProductRepository(shop);
            var variantDataRepository = this._multitenantFactory.MakeVariantRepository(shop);
            var orderRepository = this._multitenantFactory.MakeShopifyOrderRepository(shop);

            // Get all existing Variant and ProfitWise Products
            var masterVariants = variantDataRepository.RetrieveAllMasterVariants();
            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            masterProductCatalog.LoadMasterVariants(masterVariants);
            
            // GET ALL ORDER LINE ITEM MAPPINGS AND DELETE PRODUCTS / VARIANTS NOT USED
        }


    }
}

