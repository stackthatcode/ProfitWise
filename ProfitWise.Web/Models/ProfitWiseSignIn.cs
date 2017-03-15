using ProfitWise.Data.Utility;
using Push.Shopify.Model;

namespace ProfitWise.Web.Models
{
    public class ProfitWiseSignIn
    {
        public string AccessToken { get; set; }
        public Shop Shop { get; set; }

        public string AspNetUserName => Shop.Domain.ShopNameFromDomain();
    }
}