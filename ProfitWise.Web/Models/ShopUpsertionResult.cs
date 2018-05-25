namespace ProfitWise.Web.Models
{
    public class ShopUpsertionResult
    {
        public bool Success { get; set; }
        public ShopUpsertionFailureReason Reason { get; set; }
        public string PublicDescription { get; set; }


        public static ShopUpsertionResult Succeed()
        {
            return new ShopUpsertionResult() { Success = true };
        }

        public static ShopUpsertionResult Fail(
                ShopUpsertionFailureReason reason, string description = null)
        {
            return new ShopUpsertionResult()
            {
                Success = false,
                Reason = reason,
                PublicDescription = description,
            };
        }
    }

    public enum ShopUpsertionFailureReason
    {
        Exception = 1,
        ShopCurrencyChange = 2,
    }
}
