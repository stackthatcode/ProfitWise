﻿namespace ProfitWise.Data.Model.GoodsOnHand
{
    public class Totals
    {
        public long ReportId { get; set; }
        public decimal TotalCostOfGoodsOnHand { get; set; }
        public decimal TotalPotentialRevenue { get; set; }
        public decimal TotalPotentialProfit => TotalPotentialRevenue - TotalCostOfGoodsOnHand;
    }
}
