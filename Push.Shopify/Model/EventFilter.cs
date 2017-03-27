using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Model
{
    public class EventFilter
    {
        public DateTime? CreatedAtMin { get; set; }
        public string Verb { get; set; }
        public string Filter { get; set; }


        public EventFilter()
        {
            CreatedAtMin = DateTime.Today;
        }

        public override string ToString()
        {
            return $"Event Filter dump: CreatedAtMin: {CreatedAtMin} - Verb: {Verb} - Filter: {Filter}";
        }

        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = new QueryStringBuilder();

            if (CreatedAtMin != null)
            {
                builder.Add("created_at_min", CreatedAtMin.Value);
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
