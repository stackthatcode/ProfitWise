namespace ProfitWise.Data.Model.Preferences
{
    public class ProfitRealizationConstants
    {
        // Includes Profit -> PaymentStatus.Notcleared, PaymentStatus.Cleared
        public const int OrderReceived = 1;

        // Includes Profit -> PaymentStatus.Cleared (only)
        public const int PaymentClears = 2; 
    }
}
