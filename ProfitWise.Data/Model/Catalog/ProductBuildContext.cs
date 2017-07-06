using System.Collections.Generic;
using System.Linq;
using Push.Shopify.Model;

namespace ProfitWise.Data.Model.Catalog
{
    public class ProductBuildContext
    {
        public IList<PwMasterProduct> ExistingMasterProducts { get; set; }

        public bool IsActive { get; set; }
        public string Title { get; set; }
        public long? ShopifyProductId { get; set; }
        public string Vendor { get; set; }
        public string Tags { get; set; }
        public string ProductType { get; set; }

        public PwMasterProduct TargetMasterProduct { get; set; }
        

        public ProductBuildContext(Product shopifyProduct, IList<PwMasterProduct> masterProducts)
        {
            ExistingMasterProducts = masterProducts;

            // Because the product was found in Shopify, the by default it's IsActive = true
            IsActive = true;

            Title = shopifyProduct.Title;
            ShopifyProductId = shopifyProduct.Id;
            Vendor = shopifyProduct.Vendor;
            Tags = shopifyProduct.Tags;
            ProductType = shopifyProduct.ProductType;
        }

        public ProductBuildContext(OrderLineItem line, IList<PwMasterProduct> masterProducts)
        {
            ExistingMasterProducts = masterProducts;

            IsActive = false;

            Title = line.ProductTitle ?? "";
            ShopifyProductId = line.ProductId;
            Vendor = line.Vendor ?? "";
            Tags = "";
            ProductType = "";
        }
    }
}
