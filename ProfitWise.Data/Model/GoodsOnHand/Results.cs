using System.Collections.Generic;

namespace ProfitWise.Data.Model.GoodsOnHand
{
    public class Results
    {
        public int CurrencyId { get; set; }
        public Totals Totals { get; set; }
        public List<Details> Details { get; set; }
        public List<ChartData> Chart { get; set; }
        public int DetailsCount { get; set; }
    }
}
