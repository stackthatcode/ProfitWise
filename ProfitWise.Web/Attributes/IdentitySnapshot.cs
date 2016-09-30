using System;
using System.Collections.Generic;
using ProfitWise.Data.Model;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Web.Attributes
{
    public class IdentitySnapshot
    {
        // ASP.NET Identity
        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }

        public string RolesFlattened => String.Join(",", Roles);
        public bool IsAdmin => Roles?.Contains(SecurityConfig.AdminRole) ?? false;

        // Shopify 
        public string ShopDomain { get; set; }
        public string ShopName => ShopDomain.ShopName();
        public bool Impersonated { get; set; }

        // ProfitWise Shop
        public PwShop PwShop { get; set; }
    }
}
