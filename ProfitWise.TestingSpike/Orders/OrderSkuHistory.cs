namespace ProfitWise.Batch.OrderRefresh
{
    public class OrderSkuHistory
    {
        public int LineId { get; set; }
        public string OrderNumber { get; set; }
        public string ProductSku { get; set; }
        public decimal Price { get; set; }
        public decimal COGS { get; set; }
    }
}
