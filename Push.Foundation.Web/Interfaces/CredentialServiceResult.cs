namespace Push.Foundation.Web.Interfaces
{
    public class CredentialServiceResult
    {
        public CredentialServiceResult()
        {
            
        }

        public CredentialServiceResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public string Message { get; set; }
        public bool Success { get; set; }

        public string ShopOwnerUserId { get; set; }
        public bool Impersonated { get; set; }
        public string ShopDomain { get; set; }
        public string AccessToken { get; set; }
    }

}
