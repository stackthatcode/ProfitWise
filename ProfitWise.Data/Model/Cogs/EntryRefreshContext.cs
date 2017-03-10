using System.Collections.Generic;

namespace ProfitWise.Data.Model.Cogs
{
    public class EntryRefreshContext
    {
        public long? PwPickListId { get; set; }
        public long? PwMasterProductId { get; set; }
        public long? PwMasterVariantId { get; set; }
        public long? ShopifyOrderId { get; set; }
    }
}
