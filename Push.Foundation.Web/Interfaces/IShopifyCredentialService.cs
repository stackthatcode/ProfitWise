using Microsoft.AspNet.Identity.Owin;
using Push.Foundation.Web.Identity;

namespace Push.Foundation.Web.Interfaces
{
    public interface IShopifyCredentialService
    {
        CredentialServiceResult Retrieve(string currentUserId);

        void ClearUserCredentials(string userId);
        void SetUserCredentials(string userId, ExternalLoginInfo externalLoginInfo);
        void SetUserCredentials(string userId, string shopName, string unencryptedAccessToken);

        void ClearAdminImpersonation(string userId);
        void SetAdminImpersonation(string adminUserId, string shopOwnerId);
    }
}
