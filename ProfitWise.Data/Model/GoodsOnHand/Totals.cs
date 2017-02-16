namespace ProfitWise.Data.Model.GoodsOnHand
{
    public class Totals
    {
        public long ReportId { get; set; }
        public decimal TotalCostOfGoodsOnHand { get; set; }
        public decimal PotentialRevenue { get; set; }
        public decimal PotentialProfit => PotentialRevenue - TotalCostOfGoodsOnHand;
    }
}
