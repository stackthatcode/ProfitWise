namespace ProfitWise.Data.Model.Reports
{
    public class MasterVariantOption
    {
        public long PwMasterProductId { get; set; }
        public long Key => PwMasterProductId;
        public string Title => VariantTitle;

        public string Vendor { get; set; }
        public string ProductTitle { get; set; }
        public long PwMasterVariantId { get; set; }
        public string VariantTitle { get; set; }
        public string Sku { get; set;  }
    }
}
