using System.Collections.Generic;
using ProfitWise.Data.Model.Cogs;


namespace ProfitWise.Web.Models
{
    public class PwCogsMasterProductModel
    {
        public string Title { get; set; }
        public long PwMasterProductId { get; set; }
        public IList<PwCogsVariantSummary> MasterVariants { get; set; }
        public string ProductType { get; set; }
        public string Vendor { get; set; }
    }
}
