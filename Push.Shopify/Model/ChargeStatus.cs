using System.Collections.Generic;

namespace Push.Shopify.Model
{
    public enum ChargeStatus
    {
        Pending = 1,
        Accepted = 2,
        Active = 3,
        Declined = 4,
        Expired = 5,
        Frozen = 6,
        Cancelled = 7,
    }

    public static class ChargeStatusExtensions
    {
        private static readonly Dictionary<string, ChargeStatus> 
                _lookup = new Dictionary<string, ChargeStatus>
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

        public static bool IsValid(this ChargeStatus? input)
        {
            return input.HasValue && (input == ChargeStatus.Active || input == ChargeStatus.Accepted);
        }

        public static bool UserMustLoginAgain(this ChargeStatus? input)
        {
            return !input.HasValue || (
                       input.Value == ChargeStatus.Pending ||
                       input.Value == ChargeStatus.Declined ||
                       input.Value == ChargeStatus.Expired ||
                       input.Value == ChargeStatus.Cancelled);
        }

        public static bool UserMustContactSupport(this ChargeStatus? input)
        {
            return input.HasValue && input.Value == ChargeStatus.Frozen;
        }

        public static bool SystemMustCreateNewCharge(this ChargeStatus? input)
        {
            return !input.HasValue || input.Value.SystemMustCreateNewCharge();
        }

        public static bool SystemMustCreateNewCharge(this ChargeStatus input)
        {
            return input == ChargeStatus.Declined ||
                    input == ChargeStatus.Expired ||
                    input == ChargeStatus.Cancelled;
        }

    }
}
