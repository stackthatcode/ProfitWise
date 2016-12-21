using Newtonsoft.Json;

namespace ProfitWise.Data.Model.Profit
{
    public class GroupedTotal
    {
        [JsonIgnore]
        public GroupingKey GroupingKey { get; set; }

        public string GroupingId { get; set; }
        public string GroupingName { get; set; }

        public decimal TotalCogs { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit => TotalRevenue - TotalCogs;
        public int TotalNumberSold { get; set; }
    }
}
