using System.Collections.Generic;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Steps
{
    public class OrderRefreshContext
    {
        public PwShop ShopifyShop { get; set; }
        public IList<PwMasterProduct> Products { get; set; }
        public IList<ShopifyOrder> CurrentExistingOrders { get; set; }


        public void AddNewPwProduct(PwProduct product)
        {
            // PwProducts.Add(product);
        }
    }
}
