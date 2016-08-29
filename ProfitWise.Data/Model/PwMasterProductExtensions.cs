using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.General;

namespace ProfitWise.Data.Model
{
    public static class PwMasterProductExtensions
    {
        public static void LoadMasterVariants(this IList<PwMasterProduct> masterProducts, IList<PwMasterVariant> masterVariants)
        {
            masterProducts
                .SelectMany(x => x.Products)
                .ForEach(
                    product =>
                    {
                        var childMasterVariants = 
                            masterVariants.Where(mv => mv.PwProductId == product.PwProductId).ToList();

                        product.MasterVariants = childMasterVariants;
                        childMasterVariants.ForEach(x => x.ParentProduct = product);
                    });
        }
    }
}
