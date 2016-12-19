﻿using System;
using System.Collections.Generic;

namespace ProfitWise.Data.Model.Profit
{
    public class Summary
    {
        public int CurrencyId { get; set; }
        public ExecutiveSummary ExecutiveSummary { get; set; }

        public IList<GroupedTotal> ProductsByMostProfitable { get; set; }
        public IList<GroupedTotal> VendorsByMostProfitable { get; set; }
        public IList<GroupedTotal> VariantByMostProfitable { get; set; }
        public IList<GroupedTotal> ProductTypeByMostProfitable { get; set; }

        public IList<GroupedTotal> TotalsByGroupedId(ReportGrouping groupingId)
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