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
        public decimal Cost { get; set; }
        public int Inventory { get; set; }
        public string InventoryManagement { get; set; }
        public long InventoryItemId { get; set; }

        public bool InventoryTracked => InventoryManagement != null;
        public DateTime UpdatedAt { get; set; }
    }
}
