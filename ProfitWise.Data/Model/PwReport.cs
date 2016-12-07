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
        }

        public long PwReportId { get; set; }
        public long PwShopId { get; set; }

        public string Name { get; set; }        
        public bool CopyForEditing { get; set; }
        public bool SystemReport { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }    
        public ReportGrouping Grouping { get; set; }
        public ReportOrdering Ordering { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastAccessedDate { get; set; }


        public IList<string> ProductTypes { get; set; }
        public IList<string> Vendors { get; set; }
        public IList<long> MasterProductIds { get; set; }
        public IList<string> Skus { get; set; }        
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
                            { ReportOrdering.ProfitabilityDescending, "Variant (most to least)" },
                };

        public static object AllGroupingToNamedObject()
        {
            return _groupingDescriptions.Select(x => new {ReportGroupingId = x.Key, Description = x.Value}).ToList();
        }

        public static Dictionary<ReportOrdering, string> AllReportOrdering()
        {
            return _orderingDescriptions;
        }

        public static string ReportGroupingDescription(this ReportGrouping input)
        {
            return _groupingDescriptions[input];
        }

        public static string ReportOrderingDescription(this ReportOrdering input)
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
                CopyForEditing = false,
                SystemReport = true,
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today,
                Grouping = ReportGrouping.Product,
                Ordering = ReportOrdering.ProfitabilityDescending,
            };
        }

        public static PwReport TestReport()
        {
            return new PwReport
            {
                PwReportId = TestReportId,
                Name = "Test Report",
                CopyForEditing = false,
                SystemReport = true,
                StartDate = DateTime.Today.AddDays(-14),
                EndDate = DateTime.Today.AddDays(-7),
                Grouping = ReportGrouping.Product,
                Ordering = ReportOrdering.ProfitabilityDescending,
            };
        }
    }
}
