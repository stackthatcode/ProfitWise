using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitWise.Data.Model
{
    public class VariantData
    {
        public string UserId { get; set; }
        public long ShopifyVariantId { get; set; }
        public string Sku { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }

        public ProductData ParentProduct { get; set; }
    }
}
