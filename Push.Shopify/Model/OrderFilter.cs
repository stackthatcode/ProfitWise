using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Model
{
    public class OrderFilter
    {
        public OrderFilter()
        {
            Status = "any";
        }

        public string Status { get; set;  }

        public DateTime? CreatedAtMin { get; set; }
        public DateTime? CreatedAtMax { get; set; }
        public DateTime? UpdatedAtMin { get; set; }

        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = 
                new QueryStringBuilder()
                    .Add("status", this.Status)
                    .Add("order", "created_at asc");

            if (CreatedAtMin != null)
            {
                builder.Add("created_at_min",
                    CreatedAtMin.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
            }
            if (CreatedAtMax != null)
            {
                builder.Add("created_at_max",
                     CreatedAtMax.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
            }
            if (UpdatedAtMin != null)
            {
                builder.Add("updated_at_min",
                     UpdatedAtMin.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
            }

            return builder;
        }
    }
}
