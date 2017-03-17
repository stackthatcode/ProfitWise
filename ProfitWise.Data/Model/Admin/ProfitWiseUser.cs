using System;
using Push.Shopify.Model;

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
        public DateTime? ProductsLastUpdated { get; set; }
        public DateTime? OrderDatasetStart { get; set; }
        public DateTime? OrderDatasetEnd { get; set; }


        public bool IsAccessTokenValid { get; set; }
        public bool IsProfitWiseInstalled { get; set; }
        public bool IsDataLoaded { get; set; }
        public ChargeStatus? LastBillingStatus { get; set; }
        public bool IsBillingValid => LastBillingStatus.IsValid();
        public int? TempFreeTrialOverride { get; set; }
        public DateTime? UninstallDateTime { get; set; }
        public string InitialRefreshJobId { get; set; }
        public string RoutineRefreshJobId { get; set; }


        // Set downstream...
        public string CurrencyText { get; set; }
    }
}
