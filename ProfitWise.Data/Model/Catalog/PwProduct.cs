using System;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Catalog
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
        public string VendorText => Vendor.IsNullOrEmptyAlt("(No Vendor)");

        public string ProductType { get; set; }
        public string Tags { get; set; }
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsPrimaryManual { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
