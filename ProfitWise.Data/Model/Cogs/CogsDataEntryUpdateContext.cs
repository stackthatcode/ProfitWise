using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Cogs
{
    public class CogsDataEntryUpdateContext
    {
        public long? PwMasterProductId { get; set; }
        public long? PwMasterVariantId { get; set; }
        public PwCogsDetail Defaults { get; set; }
        public IList<PwCogsDetail> Details { get; set; }
        public bool HasDetails => Details != null && Details.Count > 0;

        public PwCogsDetail FirstDetail => Details.OrderBy(x => x.CogsDate).FirstOrDefault();
        public PwCogsDetail LastDetail => Details.OrderByDescending(x => x.CogsDate).FirstOrDefault();


        public IEnumerable<PwCogsDetail> DetailsAfter(DateTime date)
        {
            return Details.Where(x => x.CogsDate > date);
        }

        public PwCogsDetail NextDetail(PwCogsDetail detail)
        {
            return DetailsAfter(detail.CogsDate).OrderBy(x => x.CogsDate).FirstOrDefault();
        }

    }
}
