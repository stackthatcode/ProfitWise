using Castle.Core.Internal;

namespace ProfitWise.Data.Model.GoodsOnHand
{
    public class Details
    {
        public string GroupingKey { get; set; }
        public string GroupingName { get; set; }
        public string GroupingNameCleaned => GroupingName.IsNullOrEmpty() ? "(none)" : GroupingName;
        public int TotalInventory { get; set; }
        public decimal MinimumPrice { get; set; }
        public decimal MaximumPrice { get; set; }
        public decimal TotalCostOfGoodsSold { get; set; }
        public decimal TotalPotentialRevenue { get; set; }
        public decimal TotalPotentialProfit { get; set; }        
    }
}
