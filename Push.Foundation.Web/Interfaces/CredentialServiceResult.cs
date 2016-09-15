namespace Push.Foundation.Web.Interfaces
{
    public class CredentialServiceResult
    {
        public string Message { get; set; }
        public bool Success { get; set; }

        public string ShopOwnerUserId { get; set; }
        public bool Impersonated { get; set; }

        public string AccessToken { get; set; }
        public string ShopDomain { get; set; }
    }

}
