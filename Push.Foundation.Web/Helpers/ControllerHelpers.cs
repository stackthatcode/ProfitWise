using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Push.Foundation.Web.Helpers
{
    public static class ControllerHelpers
    {
        
        public static void AddErrors(this Controller controller, IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                controller.ModelState.AddModelError("", error);
            }
        }

        public static string ExtractUserId(this IPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;
            if (identity != null && identity.GetUserId() != null)
            {
                return identity.GetUserId();
            }
            else
            {
                return null;
            }
        }

        public static string ExtractUserId(this HttpContext context)
        {
            var claimsPrincipal = HttpContext.Current.User as ClaimsPrincipal;
            return claimsPrincipal == null ? null : claimsPrincipal.ExtractUserId();
        }
    }
}