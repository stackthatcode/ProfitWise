namespace ProfitWise.Data.Model
{
    public class PwFilter
    {
        public long PwFilterId { get; set; }
        public long PwShopId { get; set; }
        public string Name { get; set; }
        public bool AllProductTypes { get; set; }
        public bool AllVendors { get; set; }
        public bool AllProducts { get; set; }
        public bool AllSkus { get; set; }
    }
}

