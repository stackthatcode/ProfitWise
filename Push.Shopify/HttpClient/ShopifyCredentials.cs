namespace Push.Shopify.HttpClient
{
    public class ShopifyCredentials
    {
        public string ShopOwnerUserId { get; set; }
        public string ShopDomain { get; set; }
        public string AccessToken { get; set; }
        public string ShopBaseUrl => ShopUrlFromDomain(ShopDomain);


        // These are set by the Request Factory
        public string Key { get; set; }
        public string Secret { get; set; }

        private static string ShopUrlFromDomain(string domain)
        {
            return $"https://{domain}";
        }
    }
}
