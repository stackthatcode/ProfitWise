using System;
using Push.Foundation.Web.Helpers;

namespace Push.Shopify.Model
{
    public enum ShopifySortOrder
    {
        Ascending = 1,
        Descending = 2,
    };


    public class OrderFilter
    {
        public OrderFilter()
        {
            Status = "any";
            OrderByClause = "processed_at asc";
        }

        private ShopifySortOrder _shopifySortOrder;
        private string _orderByClause;


        public ShopifySortOrder ShopifySortOrder
        {
            get { return _shopifySortOrder; }
            private set { _shopifySortOrder = value; }
        }

        public string OrderByClause
        {
            get { return _orderByClause;  }
            private set { _orderByClause = value; }
        }

        public string Status { get; set;  }

        // At time of writing, these are all represented in the actual Shop's timezone i.e. the offset is implicit
        public DateTime? ProcessedAtMinUtc { get; set; }
        public DateTime? ProcessedAtMaxUtc { get; set; }
        public DateTime? UpdatedAtMinUtc { get; set; }
        public DateTime? UpdatedAtMaxUtc { get; set; }


        public override string ToString()
        {
            return $"Order Filter dump: ProcessedAtMin: {ProcessedAtMinUtc} - ProcessedAtMax: {ProcessedAtMaxUtc} - UpdatedAtMin: {UpdatedAtMinUtc}";
        }

        public OrderFilter OrderByUpdateAtDescending()
        {
            _orderByClause = "updated_at desc";
            _shopifySortOrder = ShopifySortOrder.Descending;
            return this;
        }

        public OrderFilter OrderByUpdateAtAscending()
        {
            _orderByClause = "updated_at asc";
            _shopifySortOrder = ShopifySortOrder.Ascending;
            return this;
        }
        
        public QueryStringBuilder ToQueryStringBuilder()
        {
            var builder = 
                new QueryStringBuilder()
                    .Add("status", this.Status)
                    .Add("order", this.OrderByClause);

            if (ProcessedAtMinUtc != null)
            {
                builder.Add("processed_at_min", ProcessedAtMinUtc.Value);
            }
            if (ProcessedAtMaxUtc != null)
            {
                builder.Add("processed_at_max", ProcessedAtMaxUtc.Value);
            }
            if (UpdatedAtMinUtc != null)
            {
                builder.Add("updated_at_min", UpdatedAtMinUtc.Value);
            }
            if (UpdatedAtMaxUtc != null)
            {
                builder.Add("updated_at_max", UpdatedAtMaxUtc.Value);
            }
            
            return builder;
        }
    }
}
