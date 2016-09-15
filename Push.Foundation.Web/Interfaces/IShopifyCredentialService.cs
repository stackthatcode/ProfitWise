namespace Push.Foundation.Web.Interfaces
{
    public interface IShopifyCredentialService
    {
        CredentialServiceResult Retrieve(string currentUserId);
        void ClearUserCredentials(string userId);
        void ClearAdminImpersonation(string userId);
        void SetUserCredentials(string userId, string shopName, string unencryptedAccessToken);
        void SetAdminImpersonation(string userId, string shopOwnerId);
        string RetrieveClaimValue(string userId, string claimId);
        void RemoveClaim(string userId, string claimId);
        void AddClaim(string userId, string claimId, string claimValue);
    }
}
