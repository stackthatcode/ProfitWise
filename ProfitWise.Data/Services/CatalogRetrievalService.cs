using System.Collections.Generic;
using System.Data;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Shop;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.Services
{
    [Intercept(typeof(ShopRequired))]
    public class CatalogRetrievalService : IShopFilter
    {
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly IPushLogger _logger;

        public PwShop PwShop { get; set; }
        
        public CatalogRetrievalService(
                    ConnectionWrapper connectionWrapper, 
                    MultitenantFactory multitenantFactory,
                    IPushLogger logger)
        {
            _connectionWrapper = connectionWrapper;
            _multitenantFactory = multitenantFactory;
            _logger = logger;
        }

        public IDbTransaction Transaction { get; set; }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }
        
        public IList<PwMasterProduct> RetrieveFullCatalog()
        {
            _logger.Debug("RetrieveFullCatalog - start");
            
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var variantDataRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);

            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            var masterVariants = variantDataRepository.RetrieveMasterVariantsAlt();
            var cogsDetails = cogsRepository.RetrieveCogsDetailAll();
            masterProductCatalog.LoadMasterVariants(masterVariants);
            masterVariants.LoadCogsDetail(cogsDetails);

            _logger.Debug("RetrieveFullCatalog - finish");

            return masterProductCatalog;
        }

        public PwMasterProduct RetrieveMasterProduct(long masterProductId)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var variantDataRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);

            var masterProduct = productRepository.RetrieveMasterProduct(masterProductId);
            var masterVariants = variantDataRepository.RetrieveMasterVariants(pwMasterProductId: masterProductId);
            var cogsDetails = cogsRepository.RetrieveCogsDetailByMasterProduct(masterProductId);

            masterProduct.MasterVariants = masterVariants;
            masterVariants.LoadCogsDetail(cogsDetails);

            return masterProduct;
        }
    }
}
