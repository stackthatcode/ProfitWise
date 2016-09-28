using System.Collections.Generic;
using ProfitWise.Data.Model;

namespace ProfitWise.Web.Models
{
    public class BulkEditProductCogsModel
    {
        public long MasterProductId { get; set; }
        public string Title { get; set; }
        public IList<PwCogsVariant> Variant { get; set; }
        public int CurrencyId { get; set; }
    }
}
