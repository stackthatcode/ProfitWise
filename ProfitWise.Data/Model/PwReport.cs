using System;
using System.Collections.Generic;

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
        public bool Saved { get; set; }
        public bool AllProductTypes { get; set; }
        public bool AllVendors { get; set; }
        public bool AllProducts { get; set; }
        public bool AllSkus { get; set; }
        public string Grouping { get; set; }
        public string Ordering { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastAccessedDate { get; set; }


        // Decorated data i.e. not pulled from SQL by Dapper
        public bool UserDefined { get; set; }

        public IList<string> ProductTypes { get; set; }
        public IList<string> Vendors { get; set; }
        public IList<long> MasterProductIds { get; set; }
        public IList<string> Skus { get; set; }


        public string FilterDigest =>
            (AllProductTypes ? "All " : ProductTypes.Count.ToString()) + " Product Types, " +
            (AllVendors ? "All " : Vendors.Count.ToString()) + " Vendors, " +
            (AllProducts ? "All " : MasterProductIds.Count.ToString()) + " Products, " +
            (AllSkus ? "All " : Skus.Count.ToString()) + " SKUs";

        public string GroupAndOrderingDigest =>
            (Grouping == null ? "" : $"Grouped by {Grouping}, ") +
            (Ordering == null ? "" : $"Ordered by {Ordering}");
    }


    public static class PwSystemReportFactory
    {
        public const long OverallProfitabilityId = 1;
        
        public static PwReport OverallProfitability()
        {
            return new PwReport
            {
                PwReportId = OverallProfitabilityId,
                Name = "Overall Profitability",
                AllProductTypes = true,
                AllVendors = true,
                AllProducts = true,
                AllSkus = true,
                Grouping = null,
                Ordering = "Name",
                UserDefined = false,
            };
        }
    }
}
