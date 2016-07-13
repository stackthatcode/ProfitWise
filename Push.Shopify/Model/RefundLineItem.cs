using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push.Shopify.Model
{
    public class RefundLineItem
    {
        public Refund ParentRefund { get; set; }
        public long Id { get; set; }
        public long LineItemId { get; set; }
        public int RestockQuantity { get; set; }
    }
}
