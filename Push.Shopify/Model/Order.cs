using System;
using System.Collections.Generic;

namespace Push.Shopify.Model
{
    public class Order
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IList<OrderLineItem> LineItems { get; set; }
    }
}
