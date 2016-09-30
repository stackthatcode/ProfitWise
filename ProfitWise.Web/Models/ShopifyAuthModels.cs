namespace ProfitWise.Web.Models
{
    public class AuthorizationProblemModel
    {
        public string ReturnUrl { get; set; }

        public AuthorizationProblemModel(string returnUrl)
        {
            ReturnUrl = returnUrl;
        }
    }
}

