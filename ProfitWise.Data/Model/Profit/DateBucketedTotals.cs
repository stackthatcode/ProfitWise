namespace ProfitWise.Data.Model.Profit
{
    public class DateBucketedTotal
    {
        public string CanonizedIdentifier => GroupingName + ":" + DateIdentifier;

        public string DateIdentifier { get; set; }
        public string DateLabel { get; set; }
        public string GroupingName { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
    }
}
