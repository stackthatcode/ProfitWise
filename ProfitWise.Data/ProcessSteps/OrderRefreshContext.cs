﻿using System.Collections.Generic;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.ProcessSteps
{
    public class OrderRefreshContext
    {
        public PwShop ShopifyShop { get; set; }
        public IList<PwMasterProduct> MasterProducts { get; set; }
        public IList<ShopifyOrder> CurrentExistingOrders { get; set; }


        public void AddNewPwProduct(PwProduct product)
        {
            // PwProducts.Add(product);
        }
    }
}
