namespace ProfitWise.Data.Model
{
    public class PwReportMasterVariantSelection
    {
        public long PwShopId { get; set; }
        public long PwMasterProductId { get; set; }
        public long PwMasterVariantId { get; set; }

        public string ProductTitle { get; set; }
        public string VariantTitle { get; set; }
        public string Sku { get; set; }
    }
}
