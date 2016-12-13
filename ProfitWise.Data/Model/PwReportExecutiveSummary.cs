using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitWise.Data.Model
{
    public class PwReportExecutiveSummary
    {
        public int CurrencyId { get; set; }
        public int NumberOfOrders { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal Profit { get; set; }
    }
}
