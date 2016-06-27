using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push.Shopify.Model
{
    public class Variant
    {
        public Product ParentProduct { get; set; }
        public long Id { get; set; }
        public string Sku { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
    }
}
