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
            return builder;
        }
    }
}
