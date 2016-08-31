using System.Collections.Generic;

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
    }
}
