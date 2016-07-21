using System.Web;
using ProfitWise.Web.Attributes;

namespace OAuthSandbox.Attributes
{
    public static class HttpContextExtensions
    {
        public const string _key = "UserBriefCache";
        
        
        public static UserBrief RetreiveUserBriefFromContext(this HttpContextBase context)
        {
            return context.Items[_key] as UserBrief;
        }

        public static UserBrief RetreiveUserBriefFromContext(this HttpContext context)
        {
            return context.Items[_key] as UserBrief;
        }

        public static void StoreUserBriefInContext(this HttpContextBase context, UserBrief user)
        {
            context.Items[_key] = user;
        }

        public static void StoreUserBriefInContext(this HttpContext context, UserBrief user)
        {
            context.Items[_key] = user;
        }

    }
}

