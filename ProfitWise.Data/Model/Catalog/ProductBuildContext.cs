using System.Collections.Generic;
using System.Linq;
using Push.Shopify.Model;

namespace ProfitWise.Data.Model.Catalog
{
    public class ProductBuildContext
    {
        public IList<PwMasterProduct> MasterProducts { get; set; }
        public bool IsActive { get; set; }
        public string Title { get; set; }
        public long? ShopifyProductId { get; set; }
        public string Vendor { get; set; }
        public string Tags { get; set; }
        public string ProductType { get; set; }
        public List<long> ActiveShopifyVariantIds { get; set; }
    }

    public static class ProductBuildContextExtensions
    {
        public static ProductBuildContext ToProductBuildContext(
                this Product product, IList<PwMasterProduct> masterProducts, bool isActive = false)
        {
            return new ProductBuildContext
            {
                ActiveShopifyVariantIds = product.Variants.Select(x => x.Id).ToList(),
                MasterProducts = masterProducts,
                IsActive = isActive,
                Title = product.Title,
                ShopifyProductId = product.Id,
                Vendor = product.Vendor,
                Tags = product.Tags,
                ProductType = product.ProductType,
            };
        }

        public static ProductBuildContext ToProductBuildContext(
               this OrderLineItem line, IList<PwMasterProduct> masterProducts)
        {
            return new ProductBuildContext
            {
                MasterProducts = masterProducts,
                IsActive = false,
                Title = line.ProductTitle ?? "",
                ShopifyProductId = line.ProductId,
                Vendor = line.Vendor ?? "",
                Tags = "",
                ProductType = "",
            };
        }
    } 
}
