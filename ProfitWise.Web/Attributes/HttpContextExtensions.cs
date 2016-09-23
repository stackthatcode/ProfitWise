using System.Web;

namespace ProfitWise.Web.Attributes
{
    public static class HttpContextExtensions
    {
        public const string _key = "UserBriefCache";
        
        
        public static UserBrief PullUserBriefFromContext(this HttpContextBase context)
        {
            return context.Items[_key] as UserBrief;
        }

        public static UserBrief PullUserBriefFromContext(this HttpContext context)
        {
            return context.Items[_key] as UserBrief;
        }

        public static void PushUserBriefToContext(this HttpContextBase context, UserBrief user)
        {
            context.Items[_key] = user;
        }

        public static void PushUserBriefToContext(this HttpContext context, UserBrief user)
        {
            context.Items[_key] = user;
        }
    }
}

