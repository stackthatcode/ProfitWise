namespace ProfitWise.Data.Model
{
    public class PwReportKeyedSummaryTotal<T>
    {
        public T GroupingKey { get; set; }
        public string GroupingName { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit => TotalRevenue - TotalCogs;
        public int TotalNumberSold { get; set; }
    }
}
