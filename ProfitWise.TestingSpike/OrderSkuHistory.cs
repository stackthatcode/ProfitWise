using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitWise.TestingSpike
{
    public class OrderSkuHistory
    {
        public int LineId { get; set; }
        public string OrderNumber { get; set; }
        public string ProductSku { get; set; }
        public decimal Price { get; set; }
        public decimal COGS { get; set; }
    }
}
