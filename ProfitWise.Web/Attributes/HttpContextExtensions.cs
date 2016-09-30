using System.Web;

namespace ProfitWise.Web.Attributes
{
    public static class HttpContextExtensions
    {
        public const string _key = "UserBriefCache";
        
        
        public static IdentitySnapshot PullUserBriefFromContext(this HttpContextBase context)
        {
            return context.Items[_key] as IdentitySnapshot;
        }

        public static IdentitySnapshot PullUserBriefFromContext(this HttpContext context)
        {
            return context.Items[_key] as IdentitySnapshot;
        }

        public static void PushIdentitySnapshot(this HttpContextBase context, IdentitySnapshot user)
        {
            context.Items[_key] = user;
        }

        public static void PushIdentitySnapshot(this HttpContext context, IdentitySnapshot user)
        {
            context.Items[_key] = user;
        }
    }
}

