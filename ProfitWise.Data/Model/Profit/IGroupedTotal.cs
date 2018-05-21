namespace ProfitWise.Data.Model.Profit
{
    public interface IGroupedTotal
    {
        string GroupingName { get; }
        decimal TotalCogs { get; }
        decimal TotalRevenue { get; }
        decimal TotalProfit { get; }
        int TotalQuantitySold { get; }
        int TotalOrders { get; }
        decimal AverageMargin { get; }
        decimal ProfitPercentage { get; }
    }
}
