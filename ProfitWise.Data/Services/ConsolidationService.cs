using System.Data;
using System.Linq;
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
    public class ConsolidationService : IShopFilter
    {
        private readonly IPushLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly ConnectionWrapper _connectionWrapper;

        public PwShop PwShop { get; set; }
        public IDbTransaction Transaction { get; set; }


        public ConsolidationService(
                IPushLogger pushLogger, 
                MultitenantFactory multitenantFactory, 
                ConnectionWrapper connectionWrapper)
        {
            _pushLogger = pushLogger;
            _multitenantFactory = multitenantFactory;
            _connectionWrapper = connectionWrapper;
        }
        
        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }

        public void CommitTransaction()
        {
            _connectionWrapper.CommitTranscation();
        }

        public void ConsolidateWithMasterProduct(long targetMasterProductId, long inboundProductId)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var catalogBuilderService = this._multitenantFactory.MakeCatalogBuilderService(this.PwShop);

            var targetMasterProduct = RetrieveMasterProduct(targetMasterProductId);
            var inboundMasterProductId = productRepository.RetrieveMasterProductId(inboundProductId);
            var inboundMasterProduct = RetrieveMasterProduct(inboundMasterProductId);
            var inboundProduct = inboundMasterProduct.Products.First(x => x.PwProductId == inboundProductId);

            // Step #1 - re-assign Product 
            targetMasterProduct.AssignProduct(inboundProduct);
            productRepository.UpdateProductsMasterProduct(inboundProduct);
            catalogBuilderService.AssignAndWritePrimaryProduct(targetMasterProduct);

            // Step #2 - attempt to auto-consolidate MasterVariants
            foreach (var inboundVariant in inboundMasterProduct.MasterVariants.SelectMany(x => x.Variants))
            {
                // Step #2A - attempt to 
                var targetMasterVariant = 
                    targetMasterProduct.FindMasterVariant(inboundVariant.Sku, inboundVariant.Title);

                // Step #2A - if match was found, assign this Variant under the Target Master Variant
                if (targetMasterVariant != null)
                {
                    targetMasterVariant.AssignVariant(inboundVariant);
                    variantRepository.UpdateVariantsMasterVariant(inboundVariant);
                }
            }

            // Step #3 - decommission any Master Variants that don't have Variants left
            
            
            // Step #3 - determine whether or not to decommission Master Product            
            if (inboundMasterProduct.Products.Count == 0)
            {
                productRepository.DeleteMasterProduct(inboundMasterProduct);
            }



        }

        public PwMasterProduct RetrieveMasterProduct(long masterProductId)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var variantDataRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);

            var masterProduct = productRepository.RetrieveMasterProduct(masterProductId);
            var masterVariants = variantDataRepository.RetrieveMasterVariants(masterProductId);
            var cogsDetails = cogsRepository.RetrieveCogsDetailByMasterProduct(masterProductId);

            masterProduct.MasterVariants = masterVariants;
            masterVariants.LoadCogsDetail(cogsDetails);

            return masterProduct;
        }

    }
}
