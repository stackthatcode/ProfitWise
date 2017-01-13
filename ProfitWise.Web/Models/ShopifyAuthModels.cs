using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Web.Helpers;

namespace ProfitWise.Web.Models
{
    public class AuthorizationProblemModel
    {
        public string Url { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }

        public bool UrlContainsShop => !Url.ExtractQueryParameter("shop").IsNullOrEmpty();
       
        public AuthorizationProblemModel(string url)
        {
            Url = url;
        }
    }
}

