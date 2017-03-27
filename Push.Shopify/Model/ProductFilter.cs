using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Model
{
    public class ProductFilter
    {
        public DateTime ? UpdatedAtMin { get; set; }

        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = new QueryStringBuilder();

            if (UpdatedAtMin != null)
            {
                builder.Add("updated_at_min", UpdatedAtMin.Value);
            }
            return builder;
        }
    }
}
