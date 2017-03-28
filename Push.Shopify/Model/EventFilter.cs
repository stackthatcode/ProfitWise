using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Model
{
    public class EventFilter
    {
        public DateTime? CreatedAtMinUtc { get; set; }
        public string Verb { get; set; }
        public string Filter { get; set; }


        public EventFilter()
        {
            CreatedAtMinUtc = DateTime.Today;
        }

        public override string ToString()
        {
            return $"Event Filter dump: CreatedAtMin: {CreatedAtMinUtc} - Verb: {Verb} - Filter: {Filter}";
        }

        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = new QueryStringBuilder();

            if (CreatedAtMinUtc != null)
            {
                builder.Add("created_at_min", CreatedAtMinUtc.Value);
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
