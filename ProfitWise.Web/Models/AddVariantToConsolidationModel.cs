using System.Collections.Generic;
using ProfitWise.Data.Model.Cogs;

namespace ProfitWise.Web.Models
{
    public class AddVariantToConsolidationModel
    {
        public long PwMasterVariantId { get; set; }
        public IList<PwCogsVariantSummary> MasterVariants { get; set; }
    }
}
