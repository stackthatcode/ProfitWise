using System;

namespace ProfitWise.Data.Model.Admin
{
    public class ProfitWiseUser
    {
        public string UserId { get; set; } 
        public string UserName { get; set; }
        public string Email { get; set; }
        public string TimeZone { get; set; }
        public string Domain { get; set; }
        public int CurrencyId { get; set; }
        public long PwShopId { get; set; }
        public DateTime ProductsLastUpdated { get; set; }

        public bool IsAccessTokenValid { get; set; }
        public bool IsShopEnabled { get; set; }
        public bool IsBillingValid { get; set; }
        public bool IsDataLoaded { get; set; }
        public int? TempFreeTrialOverride { get; set; }

        // Set downstream...
        public string CurrencyText { get; set; }
    }
}
