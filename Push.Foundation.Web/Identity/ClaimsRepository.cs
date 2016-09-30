using System;
using System.Collections.Generic;
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

        public IList<Claim> RetrieveClaims(string userId)
        {
            return _userManager.GetClaims(userId);
        }

        public string RetrieveClaimValue(string userId, string type)
        {
            var claims = _userManager.GetClaims(userId);
            var claim = claims.FirstOrDefault(x => x.Type == type);
            return claim == null ? null : claim.Value;
        }

        public void RemoveClaim(string userId, string type)
        {
            var claims = _userManager.GetClaims(userId);
            var claim = claims.FirstOrDefault(x => x.Type == type);
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

        public void AddClaim(string userId, string type, string claimValue)
        {
            var result = _userManager.AddClaim(userId, new Claim(type, claimValue));

            if (result.Succeeded == false)
            {
                var errors = String.Join(Environment.NewLine, result.Errors);
                var message = "UserManager.AddClaim failure: " + Environment.NewLine + errors;
                throw new InvalidOperationException(message);
            }
        }
    }
}
