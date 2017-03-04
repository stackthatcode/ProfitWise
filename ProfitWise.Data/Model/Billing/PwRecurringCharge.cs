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

        public bool IsValid => LastStatus == ChargeStatus.Accepted || LastStatus == ChargeStatus.Active;

        public bool UserNeedsToLoginAgain => LastStatus == ChargeStatus.Pending || 
                                            LastStatus == ChargeStatus.Declined ||
                                            LastStatus == ChargeStatus.Expired || 
                                            LastStatus == ChargeStatus.Cancelled;

        public bool UserNeedsToContactSupport => !UserNeedsToLoginAgain;

        public bool SystemNeedsToCreateNewCharge => LastStatus == ChargeStatus.Declined ||
                                                    LastStatus == ChargeStatus.Expired ||
                                                    LastStatus == ChargeStatus.Cancelled;

        public bool IsPrimary { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }

        public string LastJson { get; set; }
    }
}
