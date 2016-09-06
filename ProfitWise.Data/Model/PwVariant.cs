namespace ProfitWise.Data.Model
{
    public class PwVariant
    {
        public long PwVariantId { get; set; }
        public long PwShopId { get; set;  }
        public long PwProductId { get; set; }
        public long PwMasterVariantId { get; set; }
        public PwMasterVariant ParentMasterVariant { get; set; }

        public long? ShopifyProductId { get; set; }
        public long? ShopifyVariantId { get; set; }

        public string Sku { get; set; }
        public string Title { get; set; }
        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }
        public int? Inventory { get; set; }

        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
    }
}
