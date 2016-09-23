using System;
using System.Collections.Generic;
using ProfitWise.Data.Model;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Web.Attributes
{
    public class UserBrief
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }

        public string RolesFlattened => String.Join(",", Roles);
        public bool IsAdmin => Roles != null ? Roles.Contains(SecurityConfig.AdminRole) : false;
        public string ShopName { get; set; }
        public string Domain { get; set; }
        public PwShop Shop { get; set; }
    }
}
