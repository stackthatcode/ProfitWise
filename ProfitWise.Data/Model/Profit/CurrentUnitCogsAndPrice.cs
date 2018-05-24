namespace ProfitWise.Data.Model.Profit
{
    public class CurrentUnitCogsAndPrice
    {
        public int PwVariantId { get; set; }
        public decimal CurrentUnitPrice { get; set; }
        public decimal CurrentUnitCogs { get; set; }
        public decimal CurrentMargin { get; set; }
        public bool StockedDirectly;
    }
}
