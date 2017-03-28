using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Model
{
    public class ProductFilter
    {
        public DateTime ? UpdatedAtMinUtc { get; set; }

        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = new QueryStringBuilder();

            if (UpdatedAtMinUtc != null)
            {
                builder.Add("updated_at_min", UpdatedAtMinUtc.Value);
            }
            return builder;
        }
    }
}
