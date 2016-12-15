using Newtonsoft.Json;

namespace ProfitWise.Data.Model
{
    public class ReportGroupedTotal
    {
        public ReportGrouping ReportGrouping { get; set; }

        [JsonIgnore]
        public long? GroupingKeyNumeric { get; set; }

        [JsonIgnore]
        public string GroupingKeyString { get; set; }

        public string GroupingKey
        {
            get
            {
                if (GroupingKeyNumeric != null)
                {
                    return GroupingKeyNumeric.ToString();
                }
                return GroupingKeyString;
            }
        }

        public string GroupingName { get; set; }

        public decimal TotalCogs { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit => TotalRevenue - TotalCogs;
        public int TotalNumberSold { get; set; }
    }
}
