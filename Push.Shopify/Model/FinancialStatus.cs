namespace Push.Shopify.Model
{
    public static class FinancialStatus
    {
        public const string pending = "pending";
        public const string authorized = "authorized";
        public const string paid = "paid";
        public const string partially_paid = "partially_paid";
        public const string refunded = "refunded";
        public const string partially_refunded = "partially_refunded";
        public const string voided = "voided";

        public static bool PaymentCaptured(this string financialStatus)
        {
            return financialStatus == paid ||
                   financialStatus == partially_paid ||
                   financialStatus == refunded ||
                   financialStatus == partially_refunded;
        }
    }
}
