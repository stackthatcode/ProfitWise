using System.Collections.Generic;

namespace ProfitWise.Data.Utility
{
    public class ShopifyOrderDiagnosticShim
    {
        public long PwShopId { get; set; }
        public List<long> OrderIds { get; set; }
    }
}

