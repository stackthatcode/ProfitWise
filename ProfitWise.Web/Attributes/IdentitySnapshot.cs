using System;
using System.Collections.Generic;
using ProfitWise.Data.Model.Shop;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Web.Attributes
{
    public class IdentitySnapshot
    {
        // ASP.NET Identity of the actual logged in User
        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }

        public string RolesFlattened => String.Join(",", Roles);
        public bool IsAdmin => Roles?.Contains(SecurityConfig.AdminRole) ?? false;

        // Shopify/ProfitWise Shop
        // 1.) If the User is the actual Shop Owner
        // 2.) If an Admin is Impersonating an actual Shop
        public string ShopDomain { get; set; }
        public string ShopName => ShopDomain.ShopName();
        public bool Impersonated { get; set; }
        public PwShop PwShop { get; set; }

        public Dictionary<int, bool> TourState { get; set; }
    }

    public static class ShopifyExtensions
    {
        public static string ShopName(this string domain)
        {
            return domain.Replace(".myshopify.com", "");
        }
    }
}
