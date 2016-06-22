using System.Collections.Generic;

namespace ProfitWise.Data.Model
{
    public class ShopifyOrder
    {
        public int ShopId { get; set; }
        public long ShopifyOrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Email { get; set; }
        public string OrderNumber { get; set;  }
        public IList<ShopifyOrderLineItem> LineItems { get; set; }
    }
}
