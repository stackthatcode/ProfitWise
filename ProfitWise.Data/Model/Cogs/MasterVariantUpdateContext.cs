using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Cogs
{
    public class MasterVariantUpdateContext
    {
        public long? PwMasterProductId { get; set; }
        public long? PwMasterVariantId { get; set; }
        public PwCogsDetail Defaults { get; set; }  // Technically, this should be a PwMasterVariant object...
        public IList<PwCogsDetail> Details { get; set; }
        public bool HasDetails => Details != null && Details.Count > 0;

        public PwCogsDetail EffectiveCogs
        {
            get
            {
                if (HasDetails)
                {
                    var detail = 
                        Details.Where(x => x.CogsDate <= DateTime.Today)
                            .OrderByDescending(x => x.CogsDate)
                            .FirstOrDefault();
                    if (detail != null)
                    {
                        return detail;
                    }
                }

                return Defaults;
            }
        }

        public static MasterVariantUpdateContext Make(
                    long? masterVariantId, CogsDto defaults, IList<CogsDto> details)
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

