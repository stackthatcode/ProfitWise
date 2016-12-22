using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace ProfitWise.Data.Model.Profit
{
    public class ReportSeries
    {
        [JsonIgnore]
        public DataGranularity Granularity { get; set; }


        public string id { get; set; }      // "3D Printer 2016", "Ultimaker 2 Plus Week of 1/1/2013", etc. 
        public string name { get; set; }    // "3D Printers", "Ultimaker 2 Plus", "Taulman" etc.
        public IList<ReportSeriesElement> data { get; set; }
    }

    public class ReportSeriesElement
    {
        [JsonIgnore]
        public DateTime StartDate { get; set; }
        [JsonIgnore]
        public DateTime EndDate { get; set; }
        [JsonIgnore]
        public string DateIdentifier { get; set; }
        [JsonIgnore]
        public ReportSeries Parent { get; set; }

        //[JsonIgnore]
        public string CanonizedIdentifier => Parent.name + ":" + this.DateIdentifier;


        // JSON Output
        public string name { get; set; }    // "2016", "Jan 2014", etc.
        public decimal y { get; set; }
        public string drilldown { get; set; }


        // "3D Printer 2016", "Ultimaker 2 Plus Week of 1/1/2013", etc. 

        public override string ToString()
        {
            return $"CanonizedIdentifier: {CanonizedIdentifier}, y: {y}";
        }
    }
}

