﻿using System;
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
        public DateTime? ProcessedAtMinShopTz { get; set; }
        public DateTime? ProcessedAtMaxShopTz { get; set; }
        public DateTime? UpdatedAtMinShopTz { get; set; }


        public override string ToString()
        {
            return $"Order Filter dump: ProcessedAtMin: {ProcessedAtMinShopTz} - ProcessedAtMax: {ProcessedAtMaxShopTz} - UpdatedAtMin: {UpdatedAtMinShopTz}";
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

            if (ProcessedAtMinShopTz != null)
            {
                builder.Add("processed_at_min", ProcessedAtMinShopTz.Value);
            }
            if (ProcessedAtMaxShopTz != null)
            {
                builder.Add("processed_at_max", ProcessedAtMaxShopTz.Value);
            }
            if (UpdatedAtMinShopTz != null)
            {
                builder.Add("updated_at_min", UpdatedAtMinShopTz.Value);
            }

            return builder;
        }
    }
}
