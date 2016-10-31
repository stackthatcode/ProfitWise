using System.Collections.Generic;
using ProfitWise.Data.Model;


namespace ProfitWise.Web.Models
{
    public class Report
    {
        public Report()
        {
            ProductTypes = new List<string>();
            Vendors = new List<string>();
            MasterProductIds = new List<long>();
            Skus = new List<string>();
            Ordering = "Name";
        }

        public long ReportId { get; set; }
        public string Name { get; set; }

        public IList<string> ProductTypes { get; set; }
        public IList<string> Vendors { get; set; }
        public IList<long> MasterProductIds { get; set; }
        public IList<string> Skus { get; set; }

        public bool AllProductTypes { get; set; }
        public bool AllVendors { get; set; }
        public bool AllMasterProductIds { get; set; }
        public bool AllSkus { get; set; }
        

        public string FilterDigest => 
            (AllProductTypes ?  "All " : ProductTypes.Count.ToString()) + " Product Types, " +
            (AllVendors ? "All " : Vendors.Count.ToString()) + " Vendors, " +
            (AllMasterProductIds ? "All " : MasterProductIds.Count.ToString()) + " Products, " +
            (AllSkus ? "All " : Skus.Count.ToString()) + " SKUs";

        public string Grouping { get; set; }
        public string Ordering { get; set; }

        public string GroupAndOrderingDigest =>
            (Grouping == null ? "" : $"Grouped by {Grouping}, ") + 
            (Ordering == null ? "" : $"Ordered by {Ordering}");
    }

    public static class ReportFactory
    {
        public static Report OverallProfitability()
        {
            return new Report
            {
                ReportId = 0,
                Name = "Overall Profitability",
                AllProductTypes = true,
                AllVendors = true,
                AllMasterProductIds = true,
                AllSkus = true,
                Grouping = null,
                Ordering = "Name",
            };
        }

        public static Report ToReport(
                this PwReport input,
                IList<string> productTypes,
                IList<string> vendors,
                IList<long> masterProducts,
                IList<string> skus)
        {
            return new Report
            {
                ReportId = input.PwReportId,
                Name = input.Name,
                AllProductTypes = input.AllProductTypes,
                AllVendors = input.AllVendors,
                AllMasterProductIds = input.AllProducts,
                AllSkus = input.AllSkus,

                ProductTypes = productTypes,
                Vendors = vendors,
                MasterProductIds = masterProducts,
                Skus = skus,
                
                Grouping = input.Grouping,
                Ordering = "Name",
            };
        }
    }
}

