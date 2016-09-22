﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model
{
    public class PwMasterProduct
    {
        public PwMasterProduct()
        {
            MasterVariants = new List<PwMasterVariant>();
        }

        public long PwMasterProductId { get; set; }
        public long PwShopId { get; set; }
        public IList<PwProduct> Products { get; set; }
        public IList<PwMasterVariant> MasterVariants { get; set; }
        

        public PwProduct DeterminePrimaryProduct()
        {
            if (Products.Count(x => x.IsPrimary) > 1 ||
                Products.Count(x => x.IsPrimaryManual) > 1)
            {
                var msg = $"Inconsistent data - Master Product {PwMasterProductId} " +
                            $"has more than one Primary / PrimaryManual Products";
                throw new Exception(msg);
            }

            var manualPrimaryProduct = Products.FirstOrDefault(x => x.IsPrimaryManual);
            if (manualPrimaryProduct != null)
            {
                return manualPrimaryProduct;
            }

            var activeProducts = Products.Where(x => x.IsActive).ToList();
            if (activeProducts.Count == 1)
            {
                return activeProducts.First();
            }
                
            return Products.OrderByDescending(x => x.LastUpdated).First();
        }
    }
}
