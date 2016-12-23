using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Profit
{
    // TODO - move these to the Web Model namespace
    public class JsonSeries
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<JsonSeriesElement> data { get; set; }
    }

    public class JsonSeriesElement
    {
        public string name { get; set; }
        public string drilldown { get; set; }
        public decimal y { get; set; }
    }

    public static class JsonSeriesExtensions
    {
        public static JsonSeries ToJsonSeries(this ReportSeries input)
        {
            return new JsonSeries
            {
                id = input.Identifier,
                name = input.GroupingName,
                data = 
                    input.Elements.Select(x =>
                        new JsonSeriesElement
                        {
                            name = x.CanonicalDateIdentifier(),
                            drilldown = x.Drilldown,
                            y = x.Amount,
                        }).ToList()
            };
        }
    }
}
