using System.Collections.Generic;

namespace Push.Shopify.Model
{
    public class ListOfOrders
    {
        public List<Order> Orders { get; set; }
        public string Link { get; set; }
    }
}
