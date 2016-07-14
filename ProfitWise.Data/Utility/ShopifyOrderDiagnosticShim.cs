using System.Collections.Generic;

namespace ProfitWise.Data.Utility
{
    public class ShopifyOrderDiagnosticShim
    {
        public long ShopId { get; set; }
        public List<long> OrderIds { get; set; }
    }
}

