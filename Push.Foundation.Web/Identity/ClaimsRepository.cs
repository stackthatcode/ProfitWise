using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace Push.Foundation.Web.Identity
{
    public class ClaimsRepository
    {
        private readonly ApplicationUserManager _userManager;

        public ClaimsRepository(ApplicationUserManager userManager)
        {
            _userManager = userManager;
        }

        public string RetrieveClaimValue(string userId, string claimId)
        {
            var claims = _userManager.GetClaims(userId);
            var claim = claims.FirstOrDefault(x => x.Type == claimId);
            return claim == null ? null : claim.Value;
        }

        public void RemoveClaim(string userId, string claimId)
        {
            var claims = _userManager.GetClaims(userId);
            var claim = claims.FirstOrDefault(x => x.Type == claimId);
            if (claim != null)
            {
                var result = _userManager.RemoveClaim(userId, claim);
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
            var result = _userManager.AddClaim(userId, new Claim(claimId, claimValue));

            if (result.Succeeded == false)
            {
                var errors = String.Join(Environment.NewLine, result.Errors);
                var message = "UserManager.AddClaim failure: " + Environment.NewLine + errors;
                throw new InvalidOperationException(message);
            }
        }
    }
}
