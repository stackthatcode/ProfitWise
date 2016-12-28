using System;
using System.Collections.Generic;
using ProfitWise.Data.Model.Reports;

namespace ProfitWise.Data.Model.Profit
{
    public class Summary
    {
        public int CurrencyId { get; set; }
        public GroupedTotal ExecutiveSummary { get; set; }

        public IList<GroupedTotal> ProductsByMostProfitable { get; set; }
        public IList<GroupedTotal> VendorsByMostProfitable { get; set; }
        public IList<GroupedTotal> VariantByMostProfitable { get; set; }
        public IList<GroupedTotal> ProductTypeByMostProfitable { get; set; }

        public IList<GroupedTotal> TotalsByGroupingId(ReportGrouping groupingId)
        {
            if (groupingId == ReportGrouping.Product)
            {
                return ProductsByMostProfitable;
            }
            if (groupingId == ReportGrouping.Variant)
            {
                return VariantByMostProfitable;
            }
            if (groupingId == ReportGrouping.ProductType)
            {
                return ProductTypeByMostProfitable;
            }
            if (groupingId == ReportGrouping.Vendor)
            {
                return VendorsByMostProfitable;
            }
            throw new ArgumentException();
        }
    }
}
