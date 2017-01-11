namespace ProfitWise.Web.Models
{
    public class AuthorizationProblemModel
    {
        public string ReturnUrl { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }

        public AuthorizationProblemModel(string returnUrl)
        {
            ReturnUrl = returnUrl;
        }
    }
}

