namespace ProfitWise.Data.Model.Profit
{
    public class CanonizedDateTotal
    {
        public string GroupingName { get; set; }
        public string DateIdentifier { get; set; }
        public string CanonizedIdentifier => GroupingName + ":" + DateIdentifier;
        public string DateLabel { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal TotalProfit => TotalRevenue - TotalCogs;
    }
}
