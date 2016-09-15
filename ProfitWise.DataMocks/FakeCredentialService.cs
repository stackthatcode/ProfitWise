using System;
using Autofac.Extras.DynamicProxy2;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Aspect;

namespace ProfitWise.DataMocks
{
    [Intercept(typeof(ShopifyCredentialRequired))]
    public class FakeCredentialService : IShopifyCredentialService
    {
        public CredentialServiceResult Retrieve(string currentUserId)
        {
            return new CredentialServiceResult()
            {
                ShopOwnerUserId = currentUserId,
                Success = true,
            };

        }

        public void ClearUserCredentials(string userId)
        {
            throw new NotImplementedException();
        }

        public void ClearAdminImpersonation(string userId)
        {
            throw new NotImplementedException();
        }

        public void SetUserCredentials(string userId, string shopName, string unencryptedAccessToken)
        {
            throw new NotImplementedException();
        }

        public void SetAdminImpersonation(string userId, string shopOwnerId)
        {
            throw new NotImplementedException();
        }

        public string RetrieveClaimValue(string userId, string claimId)
        {
            throw new NotImplementedException();
        }

        public void RemoveClaim(string userId, string claimId)
        {
            throw new NotImplementedException();
        }

        public void AddClaim(string userId, string claimId, string claimValue)
        {
            throw new NotImplementedException();
        }
    }
}
