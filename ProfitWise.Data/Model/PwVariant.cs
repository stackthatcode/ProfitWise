namespace ProfitWise.Data.Model
{
    public class PwVariant
    {
        public long PwShopId { get; set;  }
        public long PwVariantId { get; set; }
        public long PwMasterVariantId { get; set; }
        public PwVariant Parent { get; set; }

        public long? ShopifyVariantId { get; set; }
        public bool Active { get; set; }
        public bool Primary { get; set; }
    }
}
