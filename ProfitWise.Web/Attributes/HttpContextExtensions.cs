using System.Web;
using ProfitWise.Web.Models;

namespace ProfitWise.Web.Attributes
{
    public static class HttpContextExtensions
    {
        public const string Key = "ProfitWise.CommonContext";        
        
        public static IdentitySnapshot PullIdentity(this HttpContextBase context)
        {
            var commonContext = context.Items[Key] as CommonContext;
            return commonContext?.IdentitySnapshot;
        }

        public static IdentitySnapshot PullIdentity(this HttpContext context)
        {
            var commonContext = context.Items[Key] as CommonContext;
            return commonContext?.IdentitySnapshot;
        }

        public static CommonContext PullCommonContext(this HttpContextBase context)
        {
            return context.Items[Key] as CommonContext;
        }

        public static CommonContext PullCommonContext(this HttpContext context)
        {
            return context.Items[Key] as CommonContext;
        }

        public static void PushCommonContext(this HttpContextBase context, CommonContext commonContext)
        {
            context.Items[Key] = commonContext;
        }

        public static void PushCommonContext(this HttpContext context, CommonContext commonContext)
        {
            context.Items[Key] = commonContext;
        }
    }
}

