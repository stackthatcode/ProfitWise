namespace ProfitWise.Data.Model.Reports
{
    public class ReportSelectionMasterProduct
    {
        public long PwShopId { get; set; }
        public long PwMasterProductId { get; set; }

        public string Title { get; set; }
        public string Vendor { get; set; }
        public string ProductType { get; set; }
    }
}
