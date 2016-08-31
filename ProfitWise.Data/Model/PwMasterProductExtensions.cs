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
                .ForEach(
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
