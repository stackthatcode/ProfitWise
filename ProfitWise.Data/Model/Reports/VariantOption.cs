namespace ProfitWise.Data.Model.Reports
{
    public class VariantOption
    {
        public long PwProductId { get; set; }
        public long PwVariantId { get; set; }
        
        public string Vendor { get; set; }
        public string ProductType { get; set; }

        public string ProductTitle { get; set; }
        public string VariantTitle { get; set; }
        public string Sku { get; set;  }
    }
}
