using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push.Shopify.Model
{
    public class Product
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public IList<Variant> Variants { get; set; }
    }
}
