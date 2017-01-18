using System.Collections.Generic;
using Push.Shopify.Model;

namespace ProfitWise.Data.Model.Catalog
{
    public class VariantBuildContext
    {
        public IList<PwMasterProduct> AllMasterProducts { get; set; }
        public PwMasterVariant MasterVariant { get; set; }
        public PwProduct Product { get; set; }
        public PwMasterProduct MasterProduct => Product.ParentMasterProduct;

        public string Sku { get; set; }
        public string Title { get; set; }
        public long? ShopifyProductId { get; set; }
        public long? ShopifyVariantId { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public int Inventory { get; set; }
        public bool InventoryTracked { get; set; }
    }

    public static class VariantBuildContextExtensions
    {
        public static VariantBuildContext ToVariantBuildContext(
                    this Variant importedVariant,
                    bool isActive = false,
                    IList<PwMasterProduct> allMasterProducts = null,
                    PwProduct product = null, 
                    PwMasterVariant masterVariant = null)
        {
            return new VariantBuildContext
            {
                IsActive = isActive,
                AllMasterProducts = allMasterProducts,
                Sku = importedVariant.Sku,
                Title = importedVariant.Title,
                Price = importedVariant.Price,
                ShopifyProductId = importedVariant.ParentProduct.Id,
                ShopifyVariantId = importedVariant.Id,
                Product = product,
                MasterVariant = masterVariant,
                Inventory = importedVariant.Inventory,
                InventoryTracked = importedVariant.InventoryTracked,
            };
        }

        public static VariantBuildContext ToVariantBuildContext(
                    this OrderLineItem orderLineItem,
                    bool isActive = false,
                    IList<PwMasterProduct> allMasterProducts = null)
        {
            return new VariantBuildContext
            {
                IsActive = false,
                AllMasterProducts = allMasterProducts,
                Sku = orderLineItem.Sku,
                Title = orderLineItem.VariantTitle,
                Price = orderLineItem.Price,
                ShopifyProductId = orderLineItem.ProductId,
                ShopifyVariantId = orderLineItem.VariantId,
                Product = null,
                MasterVariant = null,
                Inventory = 0,
                InventoryTracked = false,
            };
        }
    }
}
