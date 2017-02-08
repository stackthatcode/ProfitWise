using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Castle.Core.Internal;
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

        
        
        public void ConsolidateProduct(long inboundProductId, long targetMasterProductId)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);
            var catalogBuilderService = this._multitenantFactory.MakeCatalogBuilderService(this.PwShop);
            var catalogRetrievalService = this._multitenantFactory.MakeCatalogRetrievalService(this.PwShop);

            var targetMasterProduct = catalogRetrievalService.RetrieveMasterProduct(targetMasterProductId);
            var inboundMasterProductId = productRepository.RetrieveMasterProductId(inboundProductId);
            var inboundMasterProduct = catalogRetrievalService.RetrieveMasterProduct(inboundMasterProductId);
            var inboundProduct = inboundMasterProduct.Products.First(x => x.PwProductId == inboundProductId);

            // Step #1 - re-assign Product 
            targetMasterProduct.AssignProduct(inboundProduct);
            productRepository.UpdateProductsMasterProduct(inboundProduct);
            catalogBuilderService.UpdatePrimary(targetMasterProduct);

            // Step #2 - extract Variants that are associated with this Product Id across all Master Variants
            var inboundVariants = 
                inboundMasterProduct.MasterVariants
                    .SelectMany(x => x.Variants)
                    .Where(x => x.PwProductId == inboundProductId)
                    .ToList();

            foreach (var inboundVariant in inboundVariants)
            {
                // Step #2A - attempt to auto-consolidate Variants
                var targetMasterVariant = 
                    targetMasterProduct.FindMasterVariant(inboundVariant.Sku, inboundVariant.Title);

                if (targetMasterVariant != null)
                {
                    // Step #2B1 - if match was found, assign this Variant under the Target Master Variant
                    targetMasterVariant.AssignVariant(inboundVariant);
                    variantRepository.UpdateVariantsMasterVariant(inboundVariant);
                    catalogBuilderService.UpdatePrimary(targetMasterVariant);
                }
                else
                {
                    // Step #2B2 - clone the original Master Variant and assign it to the Master Product
                    var newMasterVariant = inboundVariant.ParentMasterVariant.Clone();
                    targetMasterProduct.AssignMasterVariant(newMasterVariant);

                    newMasterVariant.PwMasterVariantId = 
                            variantRepository.InsertMasterVariant(newMasterVariant);

                    foreach (var detail in newMasterVariant.CogsDetails)
                    {
                        detail.PwMasterVariantId = newMasterVariant.PwMasterVariantId;                        
                        cogsRepository.InsertCogsDetails(detail);
                    }
                }
            }

            // Step #3 - decommission any Master Variants that don't have Variants left
            foreach (var masterVariant in inboundMasterProduct.MasterVariants)
            {
                if (masterVariant.Variants.Count == 0)
                {
                    variantRepository.DeleteMasterVariant(masterVariant.PwMasterVariantId);

                    // TODO - is this guy called by the Product Clean-up Service
                    cogsRepository.DeleteCogsDetail(masterVariant.PwMasterVariantId);
                }                
            }
            
            // Step #4 - if necessary decommission Master Product the inbound belongs to
            if (inboundMasterProduct.Products.Count == 0)
            {
                // Note - it should be impossible for a Product to be 
                productRepository.DeleteMasterProduct(inboundMasterProduct);
            }
        }

        public void DeconsolidateProduct(long productId)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var masterProduct = new PwMasterProduct()
            {
                PwShopId = this.PwShop.PwShopId
            };
            var newMasterProductId = productRepository.InsertMasterProduct(masterProduct);

            ConsolidateProduct(productId, newMasterProductId);
        }

        public void ConsolidateVariant(long inboundVariantId, long targetMasterVariantId)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);

            var inboundMasterVariantId = variantRepository.RetrieveMasterVariantId(inboundVariantId);
            var inboundMasterVariant =
                variantRepository.RetrieveMasterVariants(pwMasterVariantId: inboundMasterVariantId).First();

            var inboundVariant = inboundMasterVariant.Variants.First(x => x.PwVariantId == inboundVariantId);

            var targetMasterVariant =
                variantRepository.RetrieveMasterVariants(pwMasterVariantId: targetMasterVariantId).First();
            
            targetMasterVariant.AssignVariant(inboundVariant);

            variantRepository.UpdateVariantsMasterVariant(inboundVariant);

            if (inboundMasterVariant.Variants.Count == 0)
            {
                variantRepository.DeleteMasterVariant(inboundMasterVariant.PwMasterVariantId);

                // TODO - is this guy called by the Product Clean-up Service
                cogsRepository.DeleteCogsDetail(inboundMasterVariant.PwMasterVariantId);
            }
        }

        public void DeconsolidateVariant(long variantId)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);

            var masterVariantId = variantRepository.RetrieveMasterVariantId(variantId);
            var masterVariant =
                variantRepository.RetrieveMasterVariants(pwMasterVariantId: masterVariantId).First();
            var variant = masterVariant.Variants.First(x => x.PwVariantId == variantId);

            var newMasterVariant = masterVariant.Clone();
            newMasterVariant.PwMasterVariantId =
                    variantRepository.InsertMasterVariant(newMasterVariant);

            foreach (var detail in newMasterVariant.CogsDetails)
            {
                detail.PwMasterVariantId = newMasterVariant.PwMasterVariantId;
                cogsRepository.InsertCogsDetails(detail);
            }

            newMasterVariant.AssignVariant(variant);
            variantRepository.UpdateVariantsMasterVariant(variant);
        }
    }
}
