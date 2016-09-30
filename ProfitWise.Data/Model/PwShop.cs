using System;

namespace ProfitWise.Data.Model
{
    public class PwShop
    {
        public int PwShopId { get; set; }
        public string ShopOwnerUserId { get; set; }

        public long ShopifyShopId { get; set; }
        public int CurrencyId { get; set; }
        public DateTime? StartingDateForOrders { get; set; }
        public string TimeZone { get; set; }
        public bool IsAccessTokenValid { get; set; }
        public bool IsShopEnabled { get; set; }
        public bool IsDataLoaded { get; set; }
    }
}
