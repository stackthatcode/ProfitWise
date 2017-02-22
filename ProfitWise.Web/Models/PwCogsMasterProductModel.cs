using System.Collections.Generic;
using ProfitWise.Data.Model.Cogs;


namespace ProfitWise.Web.Models
{
    public class PwCogsMasterProductModel
    {
        public string Title { get; set; }
        public long PwMasterProductId { get; set; }
        public IList<PwCogsVariant> MasterVariants { get; set; }
    }
}
