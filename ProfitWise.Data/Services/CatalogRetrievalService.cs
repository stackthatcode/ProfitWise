using System.Collections.Generic;
using System.Data;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Services
{
    [Intercept(typeof(ShopRequired))]
    public class CatalogRetrievalService : IShopFilter
    {
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly MultitenantFactory _multitenantFactory;

        public PwShop PwShop { get; set; }
        
        public CatalogRetrievalService(
                ConnectionWrapper connectionWrapper, 
                MultitenantFactory multitenantFactory)
        {
            _connectionWrapper = connectionWrapper;
            _multitenantFactory = multitenantFactory;
        }

        public IDbTransaction Transaction { get; set; }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }

        public void CommitTransaction()
        {
            _connectionWrapper.CommitTranscation();
        }

        public IList<PwMasterProduct> RetrieveFullCatalog()
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var variantDataRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);

            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            var masterVariants = variantDataRepository.RetrieveMasterVariants();
            var cogsDetails = cogsRepository.RetrieveCogsDetailAll();

            masterProductCatalog.LoadMasterVariants(masterVariants);
            masterVariants.LoadCogsDetail(cogsDetails);

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
