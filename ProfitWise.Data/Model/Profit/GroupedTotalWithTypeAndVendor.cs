namespace ProfitWise.Data.Model.Profit
{
    public class GroupedTotalWithTypeAndVendor : GroupedTotal
    {
        public string ProductType { get; set; }
        public string Vendor { get; set; }
    }
}
