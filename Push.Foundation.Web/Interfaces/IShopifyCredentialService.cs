namespace Push.Foundation.Web.Interfaces
{
    public interface IShopifyCredentialService
    {
        CredentialServiceResult Retrieve(string currentUserId);
        void ClearUserCredentials(string userId);
        void ClearAdminImpersonation(string userId);
        void SetUserCredentials(string userId, string shopName, string unencryptedAccessToken);
        void SetAdminImpersonation(string adminUserId, string shopOwnerId);
    }
}
