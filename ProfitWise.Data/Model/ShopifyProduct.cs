using System.Collections.Generic;

namespace ProfitWise.Data.Model
{
    public class ShopifyProduct
    {
        public int ShopId { get; set; }
        public long ShopifyProductId { get; set; }
        public string Title { get; set; }
        public IList<ShopifyVariant> Variants { get; set; }
    }
}

