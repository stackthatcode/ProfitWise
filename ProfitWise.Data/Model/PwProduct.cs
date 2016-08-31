using System.Collections.Generic;

namespace ProfitWise.Data.Model
{
    public class PwProduct
    {
        public long PwProductId { get; set; }
        public long PwShopId { get; set; }
        public long PwMasterProductId { get; set; }
        public PwMasterProduct ParentMasterProduct { get; set; }

        public long? ShopifyProductId { get; set; }
        public string Title { get; set; }
        public string Vendor { get; set; }
        public string ProductType { get; set; }
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
        public string Tags { get; set; }
    }
}
