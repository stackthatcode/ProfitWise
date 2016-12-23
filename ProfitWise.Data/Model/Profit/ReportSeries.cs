using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace ProfitWise.Data.Model.Profit
{
    public class ReportSeries
    {
        [JsonIgnore]
        public PeriodType PeriodType { get; set; }
        public string GroupingKey { get; set; }
        public string GroupingName { get; set; }

        public string id { get; set; }      // "3D Printer 2016", "Ultimaker 2 Plus Week of 1/1/2013", etc. 
        public string name { get; set; }    // "3D Printers", "Ultimaker 2 Plus", "Taulman" etc.
        public IList<ReportSeriesElement> Elements { get; set; }
    }

    public class ReportSeriesElement
    {
        [JsonIgnore]
        public int? Year { get; set; }
        [JsonIgnore]
        public int? Quarter { get; set; }
        [JsonIgnore]
        public int? Month { get; set; }
        [JsonIgnore]
        public int? Week { get; set; }
        [JsonIgnore]
        public int? Day { get; set; }

        [JsonIgnore]
        public ReportSeries Parent { get; set; }
        [JsonIgnore]
        public ReportSeries Child { get; set; }


        //[JsonIgnore]
        public string CanonizedIdentifier => Parent.name;
        

        // JSON Output
        public string name { get; set; }    // "2016", "Jan 2014", etc.
        public decimal y { get; set; }
        public string drilldown { get; set; }


        public override string ToString()
        {
            return $"CanonizedIdentifier: {CanonizedIdentifier}, y: {y}";
        }
    }
}

