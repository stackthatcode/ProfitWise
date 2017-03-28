using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Model
{
    public class ProductFilter
    {
        public DateTime ? UpdatedAtMinShopTz { get; set; }

        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = new QueryStringBuilder();

            if (UpdatedAtMinShopTz != null)
            {
                builder.Add("updated_at_min", UpdatedAtMinShopTz.Value);
            }
            return builder;
        }
    }
}
