using System.Collections.Generic;

namespace ProfitWise.Data.Model
{
    public class ProductData
    {
        public string UserId { get; set; }
        public long ShopifyProductId { get; set; }
        public string Title { get; set; }
        public IList<VariantData> Variants { get; set; }
    }
}

