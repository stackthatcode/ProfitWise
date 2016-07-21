namespace ProfitWise.Web.Plumbing
{
    public static class ShopifyExtensions
    {
        public static string ShopName(this string domain)
        {
            return domain.Replace(".myshopify.com", "");
        }
    }
}