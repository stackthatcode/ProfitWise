using Newtonsoft.Json;

namespace ProfitWise.Data.Model.GoodsOnHand
{
    // Made to conform with the HighChart JSON spec for feeding data to a Bar Chart
    public class HighChartElement
    {
        public string name { get; set; }
        public decimal y { get; set; }
        public string drilldownurl { get; set; }
        public bool drilldown { get; set; }
    }
}
