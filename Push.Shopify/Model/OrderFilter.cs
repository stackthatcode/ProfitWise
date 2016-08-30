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
            OrderByClause = "created_at asc";
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

        public DateTime? CreatedAtMin { get; set; }
        public DateTime? CreatedAtMax { get; set; }
        public DateTime? UpdatedAtMin { get; set; }


        public override string ToString()
        {
            return $"Order Filter dump: CreatedAtMin: {CreatedAtMin} - CreatedAtMax: {CreatedAtMax} - UpdatedAtMin: {UpdatedAtMin}";
        }


        public OrderFilter OrderByCreatedAtDescending()
        {
            _orderByClause = "created_at desc";
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
