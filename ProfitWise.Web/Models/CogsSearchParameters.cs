using System.Collections.Generic;
using ProfitWise.Data.Model;

namespace ProfitWise.Web.Models
{
    public class CogsSearchParameters
    {
        public string Text { get; set; }
 
        public IList<ProductSearchFilter> Filters { get; set; }

        public SearchResultSelection ResultSelection { get; set; }
    }
}
