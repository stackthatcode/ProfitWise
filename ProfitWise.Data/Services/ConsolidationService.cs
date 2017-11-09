using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Castle.Core.Internal;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Cogs;
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
            return _connectionWrapper.InitiateTransaction();
        }
        
        
        
        public void ConsolidateProduct(long inboundProductId, long targetMasterProductId)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);
            var cogsService = this._multitenantFactory.MakeCogsService(this.PwShop);
            var catalogBuilderService = this._multitenantFactory.MakeCatalogBuilderService(this.PwShop);
            var catalogRetrievalService = this._multitenantFactory.MakeCatalogRetrievalService(this.PwShop);

            // Step #1 - assemble the players
            var targetMasterProduct = catalogRetrievalService.RetrieveMasterProduct(targetMasterProductId);
            var inboundMasterProductId = productRepository.RetrieveMasterProductByProductId(inboundProductId);
            var inboundMasterProduct = catalogRetrievalService.RetrieveMasterProduct(inboundMasterProductId);
            var inboundProduct = inboundMasterProduct.Products.First(x => x.PwProductId == inboundProductId);
            var shopCurrencyId = this.PwShop.CurrencyId;

            // Step #2 - re-assign Product 
            targetMasterProduct.AssignProduct(inboundProduct);
            productRepository.UpdateProductsMasterProduct(inboundProduct);
            catalogBuilderService.AutoUpdateAndSavePrimary(targetMasterProduct);
            
            // Step #3 - extract Variants that are associated with this Product Id across all Master Variants
            var inboundVariants = 
                inboundMasterProduct.MasterVariants
                    .SelectMany(x => x.Variants)
                    .Where(x => x.PwProductId == inboundProductId)
                    .ToList();

            foreach (var inboundVariant in inboundVariants)
            {
                // Step #4 - attempt to auto-consolidate Variants
                var targetMasterVariant = 
                        targetMasterProduct.FindMasterVariant(inboundVariant.Sku, inboundVariant.Title);                

                if (targetMasterVariant != null)
                {
                    // Step #4B1 - if match was found, assign this Variant under the Target Master Variant
                    targetMasterVariant.AssignVariant(inboundVariant);
                    variantRepository.UpdateVariantsMasterVariant(inboundVariant);                    
                    catalogBuilderService.AutoUpdateAndSavePrimary(targetMasterVariant);
                }
                else
                {
                    // Step #4B2 - clone the original Master Variant and assign it to the Master Product
                    var newMasterVariant = inboundVariant.ParentMasterVariant.Clone();
                    targetMasterProduct.AssignMasterVariant(newMasterVariant);                    
                    newMasterVariant.PwMasterVariantId = variantRepository.InsertMasterVariant(newMasterVariant);

                    var cogsDateBlocks = CogsDateBlockContext.Make(newMasterVariant, shopCurrencyId);
                    cogsService.UpdateGoodsOnHandForMasterVariant(cogsDateBlocks);
                        
                    newMasterVariant.AssignVariant(inboundVariant);
                    variantRepository.UpdateVariantsMasterVariant(inboundVariant);
                    catalogBuilderService.AutoUpdateAndSavePrimary(newMasterVariant);

                    foreach (var detail in newMasterVariant.CogsDetails)
                    {
                        detail.PwMasterVariantId = newMasterVariant.PwMasterVariantId;                        
                        cogsRepository.InsertCogsDetails(detail);
                    }
                }
            }

            // Step #5 - decommission any Master Variants that don't have Variants left
            foreach (var inboundMasterVariant in inboundMasterProduct.MasterVariants)
            {
                if (inboundMasterVariant.Variants.Count == 0)
                {
                    variantRepository.DeleteMasterVariant(inboundMasterVariant.PwMasterVariantId);
                    cogsRepository.DeleteCogsDetail(inboundMasterVariant.PwMasterVariantId);
                }
                else
                {
                    catalogBuilderService.AutoUpdateAndSavePrimary(inboundMasterVariant);
                }            
            }
            
            // Step #6 - if necessary decommission Master Product the inbound belongs to
            if (inboundMasterProduct.Products.Count == 0)
            {
                productRepository.DeleteMasterProduct(inboundMasterProduct);
            }
            else
            {
                catalogBuilderService.AutoUpdateAndSavePrimary(inboundMasterProduct);
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

        public void ConsolidateVariant(long inboundMasterVariantId, long targetMasterVariantId)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);
            var catalogService = this._multitenantFactory.MakeCatalogBuilderService(this.PwShop);

            // Get the inbound Variant and Master Variant
            var inboundMasterVariant =
                    variantRepository.RetrieveMasterVariants(pwMasterVariantId: inboundMasterVariantId).First();
            var targetMasterVariant =
                    variantRepository.RetrieveMasterVariants(pwMasterVariantId: targetMasterVariantId).First();
            
            // Perform the assignment...
            foreach (var inboundVariant in inboundMasterVariant.Variants.ToList())
            {
                targetMasterVariant.AssignVariant(inboundVariant);
                variantRepository.UpdateVariantsMasterVariant(inboundVariant);
                catalogService.AutoUpdateAndSavePrimary(targetMasterVariant);
            }

            variantRepository.DeleteMasterVariant(inboundMasterVariant.PwMasterVariantId);                
            cogsRepository.DeleteCogsDetail(inboundMasterVariant.PwMasterVariantId);
        }

        public void DeconsolidateVariant(long variantId)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);
            var catalogService = this._multitenantFactory.MakeCatalogBuilderService(this.PwShop);
            var cogsService = this._multitenantFactory.MakeCogsService(this.PwShop);

            // Retrieve the Variant and Master Variant
            var inboundMasterVariantId = variantRepository.RetrieveMasterVariantId(variantId);
            var inboundMasterVariant = 
                    variantRepository.RetrieveMasterVariants(pwMasterVariantId: inboundMasterVariantId).First();
            var inboundVariant = inboundMasterVariant.Variants.First(x => x.PwVariantId == variantId);

            // Create the new Master Variant
            var newMasterVariant = inboundMasterVariant.Clone();
            newMasterVariant.PwMasterVariantId = variantRepository.InsertMasterVariant(newMasterVariant);

            var dateBlocks = CogsDateBlockContext.Make(newMasterVariant, this.PwShop.CurrencyId);
            cogsService.UpdateGoodsOnHandForMasterVariant(dateBlocks);

            foreach (var detail in newMasterVariant.CogsDetails)
            {
                detail.PwMasterVariantId = newMasterVariant.PwMasterVariantId;
                cogsRepository.InsertCogsDetails(detail);
            }

            // Assign the Variant
            newMasterVariant.AssignVariant(inboundVariant);
            variantRepository.UpdateVariantsMasterVariant(inboundVariant);

            // Update the Primaries
            catalogService.AutoUpdateAndSavePrimary(newMasterVariant);
            catalogService.AutoUpdateAndSavePrimary(inboundMasterVariant);
        }
    }
}
