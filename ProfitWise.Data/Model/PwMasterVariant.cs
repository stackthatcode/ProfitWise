using System.Collections.Generic;

namespace ProfitWise.Data.Model
{
    public class PwMasterVariant
    {
        public PwMasterVariant()
        {
            Variants = new List<PwVariant>();
        }

        public long PwMasterVariantId { get; set; }
        public long PwShopId { get; set; }
        public long PwMasterProductId { get; set; }
        public PwMasterProduct ParentMasterProduct { get; set; }

        public bool Exclude { get; set; }
        public bool StockedDirectly { get; set; }

        public IList<PwVariant> Variants { get; set; }
    }

}
