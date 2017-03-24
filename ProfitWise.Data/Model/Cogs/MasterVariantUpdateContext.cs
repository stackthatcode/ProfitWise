using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Cogs
{
    public class MasterVariantUpdateContext
    {
        public long PwMasterVariantId { get; set; }
        public PwCogsDetail Defaults { get; set; }  // Technically, this should be a PwMasterVariant object...
        public IList<PwCogsDetail> Details { get; set; }
        public bool HasDetails => Details != null && Details.Count > 0;
        
        public static MasterVariantUpdateContext Make(
                    long masterVariantId, CogsDto defaults, IList<CogsDto> details)
        {
            var defaultsWithConstraints = defaults.ToPwCogsDetail(masterVariantId);
            var detailsWithConstraints = details?.Select(x => x.ToPwCogsDetail(masterVariantId)).ToList();

            var context = new MasterVariantUpdateContext
            {
                PwMasterVariantId = masterVariantId,
                Defaults = defaultsWithConstraints,
                Details = detailsWithConstraints,
            };

            return context;
        }
    }
}

