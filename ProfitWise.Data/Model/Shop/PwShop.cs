using System;
using ProfitWise.Data.Model.Preferences;

namespace ProfitWise.Data.Model.Shop
{
    public class PwShop
    {
        public int PwShopId { get; set; }
        public string ShopOwnerUserId { get; set; }
        public long ShopifyShopId { get; set; }

        public int CurrencyId { get; set; }
        public string TimeZone { get; set; }

        public bool IsAccessTokenValid { get; set; }
        public bool IsShopEnabled { get; set; }
        public bool IsDataLoaded { get; set; }

        public DateTime? StartingDateForOrders { get; set; }
        public bool UseDefaultMargin { get; set; }
        public decimal DefaultMargin { get; set; }
        public decimal DefaultCogsPercent => UseDefaultMargin ? (100m - DefaultMargin) / 100m : 0m;

        public int ProfitRealization { get; set; }
        public int DateRangeDefault { get; set; }


        // TODO - wire this into configuration settings?
        public static DateTime InitialOrderStartDate()
        {
            return DateTime.Today.AddMonths(-2);
        }

        public static PwShop Make(
                string shopifyUserId, long shopId, int shopCurrencyId, string shopTimeZone)
        {
            var newShop = new PwShop
            {
                ShopOwnerUserId = shopifyUserId,   // Shopify uses the email address
                ShopifyShopId = shopId,         // Long integer unique identifier
                CurrencyId = shopCurrencyId,
                TimeZone = shopTimeZone,

                IsAccessTokenValid = true,
                IsShopEnabled = true,
                IsDataLoaded = false,

                StartingDateForOrders = InitialOrderStartDate(),
                UseDefaultMargin = true,
                DefaultMargin = 20.0m,
                ProfitRealization = Preferences.ProfitRealization.OrderReceived,
                DateRangeDefault = DateRangeDefaults.Last7Days,
            };

            return newShop;
        }

    }
}
