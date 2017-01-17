using System.Collections.Generic;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Model.ShopifyImport;

namespace ProfitWise.Data.ProcessSteps
{
    public class OrderRefreshContext
    {
        public PwShop PwShop { get; set; }
        public IList<PwMasterProduct> MasterProducts { get; set; }
        public IList<ShopifyOrder> CurrentExistingOrders { get; set; }        
    }
}
