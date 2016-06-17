namespace Push.Shopify.HttpClient
{
    public class ShopifyCredentials
    {
        public string ShopOwnerId { get; set; }
        public string ShopDomain { get; set; }
        public string AccessToken { get; set; }

        public string Key { get; set; }
        public string Secret { get; set; }

        public string ShopBaseUrl => ShopUrlFromDomain(ShopDomain);

        private static string ShopUrlFromDomain(string domain)
        {
            return $"https://{domain}";
        }
    }
}
