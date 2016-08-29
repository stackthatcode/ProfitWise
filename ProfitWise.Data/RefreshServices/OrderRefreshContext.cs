using System.Collections.Generic;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.RefreshServices
{
    public class OrderRefreshContext
    {
        public PwShop ShopifyShop { get; set; }
        public IList<ShopifyVariant> ShopifyVariants { get; set; }
        public IList<PwProduct> PwProducts { get; set; }

        public void AddNewPwProduct(PwProduct product)
        {
            PwProducts.Add(product);
        }
    }
}
