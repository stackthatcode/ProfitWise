using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity.Owin;

namespace Push.Foundation.Web.Identity
{
    public static class IdentityExtensions
    {
        public static Claim ExternalClaim(this ExternalLoginInfo externalLoginInfo, string typeUrn)
        {
            return externalLoginInfo
                .ExternalIdentity.Claims.FirstOrDefault(x => x.Type == typeUrn);
        }
    }
}
