namespace ProfitWise.Data.Model.Shop
{
    public class PwTourState
    {
        public long PwShopId { get; set; }
        public bool ShowPreferences { get; set; }
        public bool ShowProducts { get; set; }
        public bool ShowProductDetails { get; set; }
        public bool ShowProductConsolidation { get; set; }
        public bool ShowProfitabilityDashboard { get; set; }
        public bool ShowEditFilters { get; set; }
        public bool ShowProfitabilityDetail { get; set; }
        public bool ShowGoodsOnHand { get; set; }
    }
}

