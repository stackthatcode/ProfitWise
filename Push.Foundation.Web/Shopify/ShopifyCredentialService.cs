using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using IEncryptionService = Push.Foundation.Web.Security.IEncryptionService;

namespace Push.Foundation.Web.Shopify
{

    public class ShopifyCredentialService : IShopifyCredentialService
    {
        private readonly ApplicationUserManager _userManager;
        private readonly IEncryptionService _encryptionService;
        private readonly IPushLogger _pushLogger;
        private readonly ClaimsRepository _claimsRepository;

        public ShopifyCredentialService(
                ApplicationUserManager userManager, 
                IEncryptionService encryptionService,
                IPushLogger pushLogger,
                ClaimsRepository claimsRepository)
        {
            _userManager = userManager;
            _encryptionService = encryptionService;
            _pushLogger = pushLogger;
            _claimsRepository = claimsRepository;
        }


        public CredentialServiceResult Retrieve(string currentUserId)
        {
            var roles = _userManager.GetRoles(currentUserId);
            var isCurrentUserIsAdmin = roles.Contains(SecurityConfig.AdminRole);

            var claims = _claimsRepository.RetrieveClaims(currentUserId);

            var shopUserId = "";
            if (isCurrentUserIsAdmin)
            {
                shopUserId = claims.ValueByType(SecurityConfig.UserImpersonationClaim);

                if (shopUserId == null)
                {
                    return new CredentialServiceResult(false,
                        "Admin User does not currently have a User selected for impersonation.");
                }
            }
            else
            {
                shopUserId = currentUserId;
            }

            var shopName = claims.ValueByType(SecurityConfig.ShopifyDomainClaim);
            if (shopName == null)
            {
                return new CredentialServiceResult(false, "Invalid/missing Shop Name");
            }

            var accessTokenEncrypted = claims.ValueByType(SecurityConfig.ShopifyOAuthAccessTokenClaim);
            if (accessTokenEncrypted == null)
            {
                return new CredentialServiceResult(false, "Invalid/missing Access Token");
            }

            // Notice: if this causes an exception to be thrown, then it's *good* - it's a significant
            // ... System failure for decryption to fail
            string accessToken = _encryptionService.Decrypt(accessTokenEncrypted);

            return new CredentialServiceResult
            {
                ShopOwnerUserId = shopUserId,
                Success = true,
                AccessToken = accessToken,
                ShopDomain = shopName,
                Impersonated = shopUserId != currentUserId,
            };
        }

        public void ClearUserCredentials(string userId)
        {
            _claimsRepository.RemoveClaim(userId, SecurityConfig.ShopifyDomainClaim);
            _claimsRepository.RemoveClaim(userId, SecurityConfig.ShopifyOAuthAccessTokenClaim);
        }

        public void ClearAdminImpersonation(string userId)
        {
            _claimsRepository.RemoveClaim(userId, SecurityConfig.UserImpersonationClaim);
        }

        public ApplicationUser SetUserCredentials(ExternalLoginInfo externalLoginInfo)
        {
            var domainClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyDomainClaimExternal);
            var accessTokenClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyOAuthAccessTokenClaimExternal);
            
            // Push the Domain and the Access Token
            return SetUserCredentials(externalLoginInfo.DefaultUserName, domainClaim.Value, accessTokenClaim.Value);
        }

        public ApplicationUser SetUserCredentials(string defaultUserName, string shopName, string unencryptedAccessToken)
        {
            var user = _userManager.FindByName(defaultUserName);
            var userId = user.Id;

            // Clear out old, potentially invalid Claims
            _claimsRepository.RemoveClaim(userId, SecurityConfig.ShopifyOAuthAccessTokenClaim);
            _claimsRepository.RemoveClaim(userId, SecurityConfig.ShopifyDomainClaim);

            _claimsRepository.AddClaim(userId, SecurityConfig.ShopifyOAuthAccessTokenClaim, _encryptionService.Encrypt(unencryptedAccessToken));
            _claimsRepository.AddClaim(userId, SecurityConfig.ShopifyDomainClaim, shopName);

            return user;
        }

        public void SetAdminImpersonation(string adminUserId, string shopOwnerId)
        {
            _claimsRepository.RemoveClaim(adminUserId, SecurityConfig.UserImpersonationClaim);
            _claimsRepository.AddClaim(adminUserId, SecurityConfig.UserImpersonationClaim, shopOwnerId);
        }
    }
}

