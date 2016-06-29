﻿using System.Collections.Generic;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Web.Models
{
    public class UserListModel
    {
        public string Message { get; set; }

        public IList<ApplicationUser> Users { get; set; }

        public ShopifyCredentialService.RetrieveResult ShopifyCredentials { get; set; }
    }
}