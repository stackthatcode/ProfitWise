﻿using System.Collections.Generic;
using ProfitWise.Data.Model.Reports;

namespace ProfitWise.Data.Model.Profit
{
    public class DatePeriodTotal
    {
        public PeriodType PeriodType { get; set; }
        public int Year { get; set; }
        public int? Quarter { get; set; }
        public int? Month { get; set; }
        public int? Week { get; set; }
        public int? Day { get; set; }

        public ReportGrouping GroupingType { get; set; }
        public string GroupingKey { get; set; }
        public string GroupingName { get; set; }

        public DatePeriodTotal Parent { get; set; }
        public List<DatePeriodTotal> Drilldown { get; set; }

        
        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal TotalProfit => TotalRevenue - TotalCogs;

        public override string ToString()
        {
            return $"Type:{GroupingType} Key:{GroupingKey} Name:{GroupingName} Period:{PeriodType} Y:{Year} " +
                $"Q:{Quarter} M:{Month} W:{Week} D:{Day} Revenue:{TotalRevenue} Cogs:{TotalCogs} Profit:{TotalProfit}";
        }
    }    
}
