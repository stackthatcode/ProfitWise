using System.Collections.Generic;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Web.Shopify;

namespace ProfitWise.Web.Models
{
    public class UserListModel
    {
        public string Message { get; set; }

        public IList<ApplicationUser> Users { get; set; }

        public CredentialServiceResult ShopifyCredentials { get; set; }
    }
}
