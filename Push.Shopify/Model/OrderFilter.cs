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
        public DateTime? ProcessedAtMin { get; set; }
        public DateTime? ProcessedAtMax { get; set; }
        public DateTime? UpdatedAtMin { get; set; }


        public override string ToString()
        {
            return $"Order Filter dump: ProcessedAtMin: {ProcessedAtMin} - ProcessedAtMax: {ProcessedAtMax} - UpdatedAtMin: {UpdatedAtMin}";
        }

        public OrderFilter OrderByProcessAtDescending()
        {
            _orderByClause = "processed_at desc";
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

            if (ProcessedAtMin != null)
            {
                builder.Add("processed_at_min", ProcessedAtMin.Value);
            }
            if (ProcessedAtMax != null)
            {
                builder.Add("processed_at_max", ProcessedAtMax.Value);
            }
            if (UpdatedAtMin != null)
            {
                builder.Add("updated_at_min", UpdatedAtMin.Value);
            }

            return builder;
        }
    }
}
