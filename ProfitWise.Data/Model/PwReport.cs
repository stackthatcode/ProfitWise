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
        public bool CopyForEditing { get; set; }
        public bool SystemReport { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }    
        public string Grouping { get; set; }
        public string Ordering { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastAccessedDate { get; set; }


        public IList<string> ProductTypes { get; set; }
        public IList<string> Vendors { get; set; }
        public IList<long> MasterProductIds { get; set; }
        public IList<string> Skus { get; set; }
        

        public string GroupAndOrderingDigest =>
            (Grouping == null ? "" : $"Grouped by {Grouping}, ") +
            (Ordering == null ? "" : $"Ordered by {Ordering}");
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
                Grouping = null,
                Ordering = "Name",  // TODO - add this shit!
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
                Grouping = null,
                Ordering = "Name",  // TODO - add this shit!
            };
        }
    }
}
