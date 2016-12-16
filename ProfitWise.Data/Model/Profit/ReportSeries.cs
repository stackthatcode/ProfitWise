using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Profit
{
    public class ReportSeries
    {
        public DataGranularity SeriesGranularity { get; set; }
        public string Name { get; set; }    // Should be the Product Type, Vendor, Product, etc.
        public IList<ReportSeriesElement> Data { get; set; }
    }

    public class ReportSeriesElement
    {
        public ReportSeries Parent { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Name { get; set; }    // Which will always be the standarized date

        public decimal Value { get; set; }


        public string DrilldownIdentifier { get; set; } // ...?

        public override string ToString()
        {
            return $"Start: {Start}, End: {End}";
        }
    }

}


