using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.Preferences;

namespace ProfitWise.Data.Model.Reports
{
    public class PwReport
    {
        public PwReport()
        {
            CopyForEditing = false;
            IsSystemReport = false;
        }

        public long PwReportId { get; set; }
        public long PwShopId { get; set; }
        
        public int ReportTypeId { get; set; }
        public string Name { get; set; }

        public bool IsSystemReport { get; set; }
        public bool CopyForEditing { get; set; }
        public long OriginalReportId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }    
        public ReportGrouping GroupingId { get; set; }
        public ReportOrdering OrderingId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastAccessedDate { get; set; }


        public PwReport MakeCopyForEditing()
        {
            return new PwReport()
            {
                PwShopId = this.PwShopId,
                ReportTypeId = this.ReportTypeId,
                Name = this.Name,
                IsSystemReport = false,
                CopyForEditing = true,
                OriginalReportId = this.PwReportId,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                GroupingId = this.GroupingId,
                OrderingId = this.OrderingId,
                CreatedDate = DateTime.Now,
                LastAccessedDate = DateTime.Now,
            };
        }

        public void PrepareToSavePermanent(string name)
        {
            CopyForEditing = false;
            Name = name;
            LastAccessedDate = DateTime.Now;
        }
    }

    public enum ReportGrouping
    {
        Overall = 1,
        ProductType = 2,
        Vendor = 3,
        Product = 4,
        Variant = 5
    }

    public enum ReportOrdering
    {
        NameAscending = 1,
        NameDescending = 2,
        ProfitabilityAscending = 3,
        ProfitabilityDescending = 4,
    }

    public static class PwReportExtensions
    {
        private static readonly
            Dictionary<ReportGrouping, string> _groupingDescriptions =
                new Dictionary<ReportGrouping, string>()
                {
                    { ReportGrouping.Overall, "Overall (no grouping)" },
                    { ReportGrouping.ProductType, "Product Type" },
                    { ReportGrouping.Vendor, "Vendor" },
                    { ReportGrouping.Product, "Product" },
                    { ReportGrouping.Variant, "Variant" },
                };

        private static readonly
            Dictionary<ReportOrdering, string> _orderingDescriptions =
                new Dictionary<ReportOrdering, string>()
                {
                            { ReportOrdering.ProfitabilityDescending, "Profitability (most to least)" },
                            { ReportOrdering.ProfitabilityAscending , "Profitability (least to most)" },
                };

        public static object AllGroupingToNamedObject()
        {
            return _groupingDescriptions.Select(x => new { GroupingId = x.Key, Description = x.Value}).ToList();
        }

        public static object AllOrderingToNamedObject()
        {
            return _orderingDescriptions.Select(x => new { OrderingId = x.Key, Description = x.Value }).ToList(); ;
        }

        public static string Description(this ReportGrouping input)
        {
            return _groupingDescriptions[input];
        }

        public static string Description(this ReportOrdering input)
        {
            return _orderingDescriptions[input];
        }
    }

}
