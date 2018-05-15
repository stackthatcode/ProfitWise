using System;
using Push.Shopify.Model;

namespace ProfitWise.Data.Model.Billing
{
    public class PwRecurringCharge
    {
        public long PwShopId { get; set; }
        public long PwChargeId { get; set; }

        public long ShopifyRecurringChargeId { get; set; }
        public string ConfirmationUrl { get; set; }
        public ChargeStatus LastStatus { get; set; }
        public string LastStatusDesctipion => LastStatus.ToString();
        
        
        public bool IsPrimary { get; set; }
        public bool MustDestroyOnNextLogin { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }

        public string LastJson { get; set; }
    }
}
