using System.Collections.Generic;

namespace Push.Shopify.Model
{
    public class Product
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public IList<Variant> Variants { get; set; }
    }
}
