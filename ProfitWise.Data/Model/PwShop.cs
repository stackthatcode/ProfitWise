using System;

namespace ProfitWise.Data.Model
{
    public class PwShop
    {
        public int PwShopId { get; set; }
        public string UserId { get; set; }

        public long ShopifyShopId { get; set; }
        public int CurrencyId { get; set; }
        public DateTime? StartingDateForOrders { get; set; }
    }
}
