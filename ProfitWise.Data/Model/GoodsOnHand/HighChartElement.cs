using ProfitWise.Data.Model.Reports;

namespace ProfitWise.Data.Model.GoodsOnHand
{
    // Made to conform with the HighChart JSON spec for feeding data to a Bar Chart
    public class HighChartElement
    {
        public ReportGrouping? grouping { get; set; }
        public string name { get; set; }
        public decimal y { get; set; }
        public string querystringbase { get; set; }
        public bool drilldown { get; set; }
    }
}
