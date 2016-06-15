using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Push.Utilities.Security;

namespace Push.Utilities.Web.Identity
{
    public class ShopifyCredentialService
    {
        public ApplicationUserManager UserManager { get; set; }
        public static IEncryptionService EncryptionService { get; set; }

        public ShopifyCredentialService(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }


        public class RetrieveResult
        {
            public string Message { get; set; }
            public bool Success { get; set; }
            public string ShopOwnerUserId { get; set; }
            public bool Impersonated { get; set; }

            public string AccessToken { get; set; }
            public string ShopDomain { get; set; }
        }

        public RetrieveResult Retrieve(string currentUserId)
        {
            var roles = UserManager.GetRoles(currentUserId);
            var isCurrentUserIsAdmin = roles.Contains(SecurityConfig.AdminRole);
            var shopUserId = "";

            if (isCurrentUserIsAdmin)
            {
                shopUserId = RetrieveClaimValue(currentUserId, SecurityConfig.UserImpersonationClaim);

                if (shopUserId == null)
                {
                    return new RetrieveResult
                    {
                        Success = false,
                        Message = "Admin User is does not currently have a User selected for impersonation."
                    };
                }
            }
            else
            {
                shopUserId = currentUserId;
            }

            var shop_name =
                    RetrieveClaimValue(shopUserId, SecurityConfig.ShopifyDomainClaim);

            if (shop_name == null)
            {
                return new RetrieveResult
                {
                    Success = false,
                    Message = "Invalid/missing Shop Name",
                };
            }

            var access_token_encrypted =
                    RetrieveClaimValue(shopUserId, SecurityConfig.ShopifyOAuthAccessTokenClaim);

            if (access_token_encrypted == null)
            {
                return new RetrieveResult
                {
                    Success = false,
                    Message = "Invalid/missing Access Token",
                };
            }

            var access_token = "";

            try
            {
                access_token = EncryptionService.Decrypt(access_token_encrypted);
            }
            catch(Exception e)
            {
                // Log that Exception, homey!
                return new RetrieveResult
                {
                    Success = false,
                    Message = "Failed to decrypt Access Token",
                };
            }


            return new RetrieveResult
            {
                ShopOwnerUserId = shopUserId,
                Success = true,
                AccessToken = access_token,
                ShopDomain = shop_name,
                Impersonated = shopUserId != currentUserId,
            };
        }

        public void ClearUserCredentials(string userId)
        {
            RemoveClaim(userId, SecurityConfig.ShopifyDomainClaim);
            RemoveClaim(userId, SecurityConfig.ShopifyOAuthAccessTokenClaim);
        }

        public void ClearAdminImpersonation(string userId)
        {
            RemoveClaim(userId, SecurityConfig.UserImpersonationClaim);
        }

        public void SetUserCredentials(string userId, string shopName, string unencryptedAccessToken)
        {
            RemoveClaim(userId, SecurityConfig.ShopifyOAuthAccessTokenClaim);
            RemoveClaim(userId, SecurityConfig.ShopifyDomainClaim);

            AddClaim(userId, SecurityConfig.ShopifyOAuthAccessTokenClaim, EncryptionService.Encrypt(unencryptedAccessToken));
            AddClaim(userId, SecurityConfig.ShopifyDomainClaim, shopName);
        }


        public void SetAdminImpersonation(string userId, string shopOwnerId)
        {
            RemoveClaim(userId, SecurityConfig.UserImpersonationClaim);
            AddClaim(userId, SecurityConfig.UserImpersonationClaim, shopOwnerId);
        }


        // Claim Helper Functions

        public string RetrieveClaimValue(string userId, string claimId)
        {
            var claims = UserManager.GetClaims(userId);
            var claim = claims.FirstOrDefault(x => x.Type == claimId);
            return claim == null ? null : claim.Value;
        }

        public void RemoveClaim(string userId, string claimId)
        {
            var claims = UserManager.GetClaims(userId);
            var claim = claims.FirstOrDefault(x => x.Type == claimId);
            if (claim != null)
            {
                var result = UserManager.RemoveClaim(userId, claim);
                if (result.Succeeded == false)
                {
                    var errors = String.Join(Environment.NewLine, result.Errors);
                    var message = "UserManager.RemoveClaim failure: " + Environment.NewLine + errors;
                    throw new InvalidOperationException(message);
                }
            }
        }

        public void AddClaim(string userId, string claimId, string claimValue)
        {
            var result = UserManager.AddClaim(userId, new Claim(claimId, claimValue));

            if (result.Succeeded == false)
            {
                var errors = String.Join(Environment.NewLine, result.Errors);
                var message = "UserManager.AddClaim failure: " + Environment.NewLine + errors;
                throw new InvalidOperationException(message);
            }
        }


    }
}

