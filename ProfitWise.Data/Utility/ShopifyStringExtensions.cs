namespace ProfitWise.Data.Utility
{
    public static class ShopifyStringExtensions
    {
        public static string ShopNameFromDomain(this string input)
        {
            return input.Replace(".myshopify.com", "");
        }
    }
}
