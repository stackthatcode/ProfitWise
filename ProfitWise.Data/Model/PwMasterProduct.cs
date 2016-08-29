using System.Collections.Generic;

namespace ProfitWise.Data.Model
{
    public class PwMasterProduct
    {
        public long PwShopId { get; set; }
        public long PwMasterProductId { get; set; }
        public IList<PwProduct> Products { get; set; }
    }
}
