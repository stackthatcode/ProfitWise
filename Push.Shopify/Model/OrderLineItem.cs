using System;
using System.Linq;

namespace Push.Shopify.Model
{
    public class OrderLineItem
    {
        public long Id { get; set; }
        public long? ProductId { get; set; }
        public long? VariantId { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public decimal Price { get; set; }

        public string ProductTitle { get; set; }
        public string VariantTitle { get; set; }
        public string Name { get; set; }
        public string Vendor { get; set; }

        public Order ParentOrder { get; set; }

        public int TotalRestockQuantity
        {
            get
            {
                return 
                    ParentOrder.Refunds
                        .SelectMany(x => x.LineItems)
                        .ToList()
                        .Where(x => x.LineItemId == this.Id)
                        .Sum(x => x.RestockQuantity);
            }
        }

    }
}
