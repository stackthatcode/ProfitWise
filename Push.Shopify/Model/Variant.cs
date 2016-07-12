using System;

namespace Push.Shopify.Model
{
    public class Variant
    {
        public Product ParentProduct { get; set; }
        public long Id { get; set; }
        public string Sku { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public int Inventory { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
