using System.Collections.Generic;

namespace ProfitWise.Data.Model.Cogs
{
    public class CogsUpdateContext
    {
        public long? MasterVariantId { get; set; }
        public PwCogsDetail Defaults { get; set; }
        public IList<PwCogsDetail> Details { get; set; }
        public bool HasDetails => Details != null && Details.Count > 0;
    }
}
