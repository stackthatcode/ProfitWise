using System.Collections.Generic;

namespace Push.Shopify.Model
{
    public enum ChargeStatus
    {
        Pending,
        Accepted,
        Active,
        Declined,
        Expired,
        Frozen,
        Cancelled,
    }

    public static class ChargeStatusExtensions
    {
        private static Dictionary<string, ChargeStatus> _lookup = new Dictionary<string, ChargeStatus>()
        {
            { "pending", ChargeStatus.Pending },
            { "accepted", ChargeStatus.Accepted },
            { "active", ChargeStatus.Active },
            { "declined", ChargeStatus.Declined },
            { "expired", ChargeStatus.Expired },
            { "frozen", ChargeStatus.Frozen },
            { "cancelled", ChargeStatus.Cancelled },
        };

        public static ChargeStatus ToChargeStatus(this string input)
        {
            return _lookup[input];
        }
    }
}
