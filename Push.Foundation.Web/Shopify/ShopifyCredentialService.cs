using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using IEncryptionService = Push.Foundation.Web.Security.IEncryptionService;

namespace Push.Foundation.Web.Shopify
{

    public class ShopifyCredentialService : IShopifyCredentialService
    {
        private readonly ApplicationUserManager _userManager;
        private readonly IEncryptionService _encryptionService;
        private readonly ClaimsRepository _claimsRepository;


        public ShopifyCredentialService(
                ApplicationUserManager userManager, IEncryptionService encryptionService, ClaimsRepository claimsRepository)
        {
            _userManager = userManager;
            _encryptionService = encryptionService;
            _claimsRepository = claimsRepository;
        }

        public string EffectiveShopUserId(string currentUserId)
        {
            var roles = _userManager.GetRoles(currentUserId);
            var isCurrentUserIsAdmin = roles.Contains(SecurityConfig.AdminRole);

            if (isCurrentUserIsAdmin)
            {
                var claims = _claimsRepository.RetrieveClaims(currentUserId);
                var shopUserId = claims.ValueByType(SecurityConfig.UserImpersonationClaim);

                if (shopUserId == null)
                {
                    throw new Exception(
                        "Admin User does not currently have a valid User selected for impersonation.");
                }

                return shopUserId;
            }
            else
            {
                return currentUserId;
            }

        }

        public CredentialServiceResult Retrieve(string currentUserId)
        {
            var shopUserId = EffectiveShopUserId(currentUserId);
            var claims = _claimsRepository.RetrieveClaims(shopUserId);
            
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

        public void SetUserCredentials(string userId, ExternalLoginInfo externalLoginInfo)
        {
            var domainClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyDomainClaimExternal);
            var accessTokenClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyOAuthAccessTokenClaimExternal);
            
            // Push the Domain and the Access Token
            SetUserCredentials(userId, domainClaim.Value, accessTokenClaim.Value);
        }

        public void SetUserCredentials(string userId, string shopName, string unencryptedAccessToken)
        {

            // Clear out old, potentially invalid Claims
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

