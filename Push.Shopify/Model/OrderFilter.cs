using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Model
{
    public class OrderFilter
    {
        public OrderFilter()
        {
            Status = "any";
            OrderByClause = "created_at asc";
        }

        public string OrderByClause { get; set; }

        public string Status { get; set;  }

        public DateTime? CreatedAtMin { get; set; }
        public DateTime? CreatedAtMax { get; set; }
        public DateTime? UpdatedAtMin { get; set; }


        public override string ToString()
        {
            return $"Order Filter dump: CreatedAtMin: {CreatedAtMin} - CreatedAtMax: {CreatedAtMax} - UpdatedAtMin: {UpdatedAtMin}";
        }


        public OrderFilter OrderByCreatedAtDescending()
        {
            OrderByClause = "created_at desc";
            return this;
        }

        public OrderFilter OrderByUpdateAtAscending()
        {
            OrderByClause = "created_at asc";
            return this;
        }



        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = 
                new QueryStringBuilder()
                    .Add("status", this.Status)
                    .Add("order", this.OrderByClause);

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
