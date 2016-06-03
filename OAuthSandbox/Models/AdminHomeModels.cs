using System.Collections.Generic;
using Push.Utilities.Web.Identity;

namespace OAuthSandbox.Models
{
    public class UserListModel
    {
        public string Message { get; set; }

        public IList<ApplicationUser> Users { get; set; }

        public ShopifyCredentialService.RetreiveResult ShopifyCredentials { get; set; }
    }
}
