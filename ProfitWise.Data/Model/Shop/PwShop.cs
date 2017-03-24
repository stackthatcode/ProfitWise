using System;
using ProfitWise.Data.Model.Preferences;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Services;
using Push.Shopify.Model;

namespace ProfitWise.Data.Model.Shop
{
    public class PwShop
    {
        public int PwShopId { get; set; }
        public string ShopOwnerUserId { get; set; }
        public long ShopifyShopId { get; set; }

        public string Domain { get; set; }
        public int CurrencyId { get; set; }
        public string TimeZone { get; set; }

        public bool IsAccessTokenValid { get; set; }
        public bool IsProfitWiseInstalled { get; set; }
        public bool IsBillingValid => LastBillingStatus.IsValid();

        public ChargeStatus? LastBillingStatus { get; set; }

        public bool IsDataLoaded { get; set; }

        public DateTime StartingDateForOrders { get; set; }

        public DateTime 
            StartingDateForOrdersInShopTime
                => StartingDateForOrders.FromUtcToShopifyTimeZone(this.TimeZone);

        public bool UseDefaultMargin { get; set; }
        public decimal DefaultMargin { get; set; }
        public decimal DefaultCogsPercent => UseDefaultMargin ? (100m - DefaultMargin) / 100m : 0m;

        public int ProfitRealization { get; set; }

        // Passed to Profit Report Entry to exclude/include Report Entries based on Payment Status
        // ... the Entries are filtered by a greater-than-or-equals of Payment Status
        public int MinPaymentStatus =>
                    ProfitRealization == ProfitRealizationConstants.PaymentClears 
                        ? PaymentStatus.Captured 
                        : PaymentStatus.NotCaptured;

        public int DateRangeDefault { get; set; }
        public int? TempFreeTrialOverride { get; set; }
        public long? ShopifyUninstallId { get; set; }
        public DateTime? UninstallDateTime { get; set; }



        public static PwShop Make(
                string shopifyUserId, long shopId, int shopCurrencyId, string shopTimeZone, 
                string shopDomain, DateTime startDateForOrders)
        {
            var newShop = new PwShop
            {
                ShopOwnerUserId = shopifyUserId,   // Shopify uses the email address
                ShopifyShopId = shopId,         // Long integer unique identifier
                CurrencyId = shopCurrencyId,
                TimeZone = shopTimeZone,
                Domain = shopDomain,

                IsAccessTokenValid = true,
                IsProfitWiseInstalled = true,
                IsDataLoaded = false,

                StartingDateForOrders = startDateForOrders,
                UseDefaultMargin = true,
                DefaultMargin = 20.0m,
                ProfitRealization = ProfitRealizationConstants.OrderReceived,
                DateRangeDefault = DateRangeDefaults.Last7Days,

                ShopifyUninstallId = null,
                UninstallDateTime = null,
            };

            return newShop;
        }

    }
}
