using System;

namespace ProfitWise.Data.Model.Catalog
{
    public class PwVariant
    {
        public long PwVariantId { get; set; }
        public long PwShopId { get; set;  }
        public long PwProductId { get; set; }
        public long PwMasterVariantId { get; set; }
        public PwMasterVariant ParentMasterVariant { get; set; }


        public override string ToString()
        {
            return
                "PwVariant" + Environment.NewLine +
                $"PwShopId = {PwShopId}" + Environment.NewLine +
                $"PwVariantId = {PwVariantId}" + Environment.NewLine +
                $"PwMasterVariantId = {PwMasterVariantId}" + Environment.NewLine +
                $"PwProductId = {PwProductId}" + Environment.NewLine +
                $"ShopifyProductId = {ShopifyProductId}" + Environment.NewLine +
                $"ShopifyVariantId = {ShopifyVariantId}" + Environment.NewLine +
                $"Sku = {Sku}" + Environment.NewLine +
                $"Title = {Title}" + Environment.NewLine;
        }


        public long? ShopifyProductId { get; set; }
        public long? ShopifyVariantId { get; set; }

        public string Sku { get; set; }
        public string Title { get; set; }
        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal? CurrentPrice { get; set; }
        public int? Inventory { get; set; }

        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsPrimaryManual { get; set; }
        public DateTime LastUpdated { get; set; }

    }
}
