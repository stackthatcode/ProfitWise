using Microsoft.AspNet.Identity.Owin;
using Push.Foundation.Web.Identity;

namespace Push.Foundation.Web.Interfaces
{
    public interface IShopifyCredentialService
    {
        CredentialServiceResult Retrieve(string currentUserId);
        void ClearUserCredentials(string userId);
        void ClearAdminImpersonation(string userId);

        ApplicationUser SetUserCredentials(ExternalLoginInfo externalLoginInfo);
        ApplicationUser SetUserCredentials(string defaultUserName, string shopName, string unencryptedAccessToken);
        void SetAdminImpersonation(string adminUserId, string shopOwnerId);
    }
}
