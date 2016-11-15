namespace ProfitWise.Data.Model
{
    public class PwReportFilter
    {
        public long PwReportId { get; set; }
        public long PwShopId { get; set; }
        public string FilterType { get; set; }
        public long NumberKey { get; set; }
        public string StringKey { get; set; }
        public int Order { get; set; }
    }
}
