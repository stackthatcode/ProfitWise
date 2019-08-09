using System.Collections.Generic;

namespace Push.Shopify.Model
{
    public class InventoryItem
    {
        public long id { get; set; }
        public decimal? cost { get; set; }
    }

    public class InventoryItemList
    {
        public List<InventoryItem> inventory_items { get; set; }
    }

}
