namespace ProfitWise.Data.Model
{
    public class PwReportSelectionMasterProduct
    {
        public long PwShopId { get; set; }
        public long PwMasterProductId { get; set; }

        public string Title { get; set; }
        public string Vendor { get; set; }
        public string ProductType { get; set; }
    }
}
