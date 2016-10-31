using System;

namespace ProfitWise.Data.Model
{
    public class PwReport
    {
        public long PwReportId { get; set; }
        public long PwShopId { get; set; }
        public string Name { get; set; }
        public bool Saved { get; set; }
        public bool AllProductTypes { get; set; }
        public bool AllVendors { get; set; }
        public bool AllProducts { get; set; }
        public bool AllSkus { get; set; }
        public string Grouping { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastAccessedDate { get; set; }
    }
}
