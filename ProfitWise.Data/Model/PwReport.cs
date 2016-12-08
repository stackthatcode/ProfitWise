using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model
{
    public class PwReport
    {
        public PwReport()
        {
            ProductTypes = new List<string>();
            Vendors = new List<string>();
            MasterProductIds = new List<long>();
            Skus = new List<string>();
            CopyForEditing = false;
            CopyOfSystemReport = false;
            IsSystemReport = false;
        }

        public long PwReportId { get; set; }
        public long PwShopId { get; set; }

        public string Name { get; set; }        
        public bool CopyForEditing { get; set; }
        public bool CopyOfSystemReport { get; set; }
        public long OriginalReportId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }    
        public ReportGrouping GroupingId { get; set; }
        public ReportOrdering OrderingId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastAccessedDate { get; set; }


        public IList<string> ProductTypes { get; set; }
        public IList<string> Vendors { get; set; }
        public IList<long> MasterProductIds { get; set; }
        public IList<string> Skus { get; set; }  
        
        public bool IsSystemReport { get; set; }    
    }

    public enum ReportGrouping
    {
        ProductType = 1,
        Vendor = 2,
        Product = 3,
        Variant = 4
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
                    { ReportGrouping.ProductType, "Product Type" },
                    { ReportGrouping.Vendor, "Vendor" },
                    { ReportGrouping.Product, "Product" },
                    { ReportGrouping.Variant, "Variant" },
                };

        private static readonly
            Dictionary<ReportOrdering, string> _orderingDescriptions =
                new Dictionary<ReportOrdering, string>()
                {
                            { ReportOrdering.NameAscending, "Name (A-to-Z)" },
                            { ReportOrdering.NameDescending, "Name (Z-to-A)" },
                            { ReportOrdering.ProfitabilityAscending , "Profitability (least to most)" },
                            { ReportOrdering.ProfitabilityDescending, "Profitability (most to least)" },
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

    public static class PwSystemReportFactory
    {
        public const long OverallProfitabilityId = 1;
        public const long TestReportId = 2;

        public static string CustomDefaultNameBuilder(int reportNumber)
        {
            return $"Untitled Report-{reportNumber}";
        }

        public static PwReport OverallProfitability()
        {
            return new PwReport
            {
                PwReportId = OverallProfitabilityId,
                Name = "Overall Profitability",
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today,
                GroupingId = ReportGrouping.ProductType,
                OrderingId = ReportOrdering.ProfitabilityDescending,
                IsSystemReport = true,
            };
        }

        public static PwReport TestReport()
        {
            return new PwReport
            {
                PwReportId = TestReportId,
                Name = "Test Report",
                StartDate = DateTime.Today.AddDays(-14),
                EndDate = DateTime.Today.AddDays(-7),
                GroupingId = ReportGrouping.Product,
                OrderingId = ReportOrdering.ProfitabilityDescending,
                IsSystemReport = true,
            };
        }
    }
}
