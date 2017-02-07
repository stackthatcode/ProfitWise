using System;
using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.General;

namespace ProfitWise.Data.Model.Catalog
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

            // Rule #1 - if a certain Product has been manually set to Primary, it's always Primary
            var manualPrimaryProduct = Products.FirstOrDefault(x => x.IsPrimaryManual);
            if (manualPrimaryProduct != null)
            {
                return manualPrimaryProduct;
            }

            // Rule #2 - if there is exactly *one* Active Products, it is automatically the Primary
            var activeProducts = Products.Where(x => x.IsActive).ToList();
            if (activeProducts.Count == 1)
            {
                return activeProducts.First();
            }
                
            // Rule #3 - the Primary that will default to the most recently updated
            return Products.OrderByDescending(x => x.LastUpdated).First();
        }
    }

    public static class PwMasterProductExtensions
    {
        public static void LoadMasterVariants(this IList<PwMasterProduct> masterProducts, IList<PwMasterVariant> masterVariants)
        {
            masterProducts.ForEach(
                    masterProduct =>
                    {
                        masterProduct.MasterVariants
                            = masterVariants
                                    .Where(mv => mv.PwMasterProductId == masterProduct.PwMasterProductId)
                                    .ToList();
                    });
        }
    }
}

