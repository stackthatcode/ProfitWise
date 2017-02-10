namespace ProfitWise.Data.Model.Reports
{
    public class PwProductSummary
    {
        public long PwMasterProductId { get; set; }
        public string Vendor { get; set; }
        public string Title { get; set; }
        public int VariantCount { get; set; }
    }
}
