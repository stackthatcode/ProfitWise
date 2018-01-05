using System.Web;
using ProfitWise.Web.Models;
using Push.Foundation.Utilities.Json;

namespace ProfitWise.Web.Attributes
{
    public static class HttpContextExtensions
    {
        public const string Key = "ProfitWise.CommonContext";


        public static string CurrentPwShopId(this HttpContextBase context)
        {
            var identity = context.IdentitySnapshot();
            return identity?.PwShop.PwShopId.ToString();
        }

        public static IdentitySnapshot IdentitySnapshot(this HttpContextBase context)
        {
            var commonContext = context.Items[Key] as AuthenticatedContext;
            return commonContext?.IdentitySnapshot;
        }

        public static IdentitySnapshot IdentitySnapshot(this HttpContext context)
        {
            var commonContext = context.Items[Key] as AuthenticatedContext;
            return commonContext?.IdentitySnapshot;
        }

        public static AuthenticatedContext AuthenticatedContext(this HttpContextBase context)
        {
            return context.Items[Key] as AuthenticatedContext;
        }

        public static AuthenticatedContext AuthenticatedContext(this HttpContext context)
        {
            return context.Items[Key] as AuthenticatedContext;
        }

        public static void AuthenticatedContext(this HttpContextBase context, AuthenticatedContext commonContext)
        {
            context.Items[Key] = commonContext;
        }

        public static void AuthenticatedContext(this HttpContext context, AuthenticatedContext commonContext)
        {
            context.Items[Key] = commonContext;
        }

        public static string CurrentUserTourStateJson(this HttpContextBase context)
        {
            return context.IdentitySnapshot().TourState.SerializeToJson();
        }
    }
}

