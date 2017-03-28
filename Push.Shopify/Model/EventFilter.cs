using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Model
{
    public class EventFilter
    {
        public DateTime? CreatedAtMinShopTz { get; set; }
        public string Verb { get; set; }
        public string Filter { get; set; }


        public EventFilter()
        {
            CreatedAtMinShopTz = DateTime.Today;
        }

        public override string ToString()
        {
            return $"Event Filter dump: CreatedAtMin: {CreatedAtMinShopTz} - Verb: {Verb} - Filter: {Filter}";
        }

        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = new QueryStringBuilder();

            if (CreatedAtMinShopTz != null)
            {
                builder.Add("created_at_min", CreatedAtMinShopTz.Value);
            }
            if (Verb != null)
            {
                builder.Add("verb", Verb);
            }
            if (Filter != null)
            {
                builder.Add("filter", Filter);
            }

            return builder;
        }
    }
}
