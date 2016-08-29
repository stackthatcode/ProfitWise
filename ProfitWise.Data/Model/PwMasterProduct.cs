using System.Collections.Generic;

namespace ProfitWise.Data.Model
{
    public class PwMasterProduct
    {
        public long ShopId { get; set; }
        public long PwMasterProductId { get; set; }
        public IList<PwProduct> Products { get; set; }
    }
}
