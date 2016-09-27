using System.Collections.Generic;

namespace ProfitWise.Web.Models
{
    public class EditProductCogsModel
    {
        public IList<string> Vendors { get; set; }
        public IList<string> ProductTypes { get; set; }
        public int CurrencyId { get; set; }
    }
}
