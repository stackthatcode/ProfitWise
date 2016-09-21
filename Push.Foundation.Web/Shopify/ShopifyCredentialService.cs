using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
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
            var shopUserId = "";

            if (isCurrentUserIsAdmin)
            {
                shopUserId = _claimsRepository.RetrieveClaimValue(currentUserId, SecurityConfig.UserImpersonationClaim);

                if (shopUserId == null)
                {
                    return new CredentialServiceResult()
                    {
                        Success = false,
                        Message = "Admin User does not currently have a User selected for impersonation."
                    };
                }
            }
            else
            {
                shopUserId = currentUserId;
            }

            var shopName = _claimsRepository.RetrieveClaimValue(shopUserId, SecurityConfig.ShopifyDomainClaim);

            if (shopName == null)
            {
                return new CredentialServiceResult()
                {
                    Success = false,
                    Message = "Invalid/missing Shop Name",
                };
            }

            var accessTokenEncrypted =
                    _claimsRepository.RetrieveClaimValue(shopUserId, SecurityConfig.ShopifyOAuthAccessTokenClaim);

            if (accessTokenEncrypted == null)
            {
                return new CredentialServiceResult
                {
                    Success = false,
                    Message = "Invalid/missing Access Token",
                };
            }

            var accessToken = "";

            try
            {
                accessToken = _encryptionService.Decrypt(accessTokenEncrypted);
            }
            catch(Exception e)
            {
                // Log that Exception, homey!
                return new CredentialServiceResult
                {
                    Success = false,
                    Message = "Failed to decrypt Access Token",
                };
            }

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

        public void SetUserCredentials(string userId, string shopName, string unencryptedAccessToken)
        {
            _claimsRepository.RemoveClaim(userId, SecurityConfig.ShopifyOAuthAccessTokenClaim);
            _claimsRepository.RemoveClaim(userId, SecurityConfig.ShopifyDomainClaim);

            _claimsRepository.AddClaim(userId, SecurityConfig.ShopifyOAuthAccessTokenClaim, _encryptionService.Encrypt(unencryptedAccessToken));
            _claimsRepository.AddClaim(userId, SecurityConfig.ShopifyDomainClaim, shopName);
        }

        public void SetAdminImpersonation(string adminUserId, string shopOwnerId)
        {
            _claimsRepository.RemoveClaim(adminUserId, SecurityConfig.UserImpersonationClaim);
            _claimsRepository.AddClaim(adminUserId, SecurityConfig.UserImpersonationClaim, shopOwnerId);
        }
    }
}

