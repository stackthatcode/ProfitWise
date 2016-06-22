using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    public class ShopifyOrderRepository : IShopIdFilter
    {
        private readonly MySqlConnection _connection;

        public ShopifyOrderRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public int? ShopId { get; set; }

        public virtual ShopifyOrder RetrieveOrders()
        {
            var query = @"SELECT * FROM shopifyorder WHERE ShopId";
            return
                _connection
                    .Query<ShopifyOrder>(query, new { ShopId.Value })
                    .FirstOrDefault();
        }

        public virtual ShopifyOrder RetrieveOrder(long shopifyOrderId)
        {
            var query = @"SELECT * FROM shopifyorder WHERE ShopId = @ShopId AND ShopifyOrderId = @shopifyOrderId";
            return _connection
                    .Query<ShopifyOrder>(query, new { ShopId, shopifyOrderId })
                    .FirstOrDefault();
        }

        public virtual void InsertOrder(ShopifyOrder order)
        {
            order.ShopId = ShopId.Value;
            var query = @"INSERT INTO shopifyorder VALUES(@ShopId, @ShopifyOrderId, @TotalPrice, @Email, @OrderNumber)";
            _connection.Execute(query, order);
        }

        public virtual void DeleteOrder(long shopifyOrderId)
        {
            var query = @"DELETE FROM shopifyorder 
                        WHERE ShopId = @ShopId AND ShopifyOrderId = @shopifyOrderId";
            _connection.Execute(query, new { ShopId, shopifyOrderId });
        }


        public virtual IList<ShopifyOrderLineItem> RetrieveOrderLineItems(long shopifyOrderId)
        {
            var query = @"SELECT * FROM shopifyorderlineitem WHERE ShopId = @ShopId AND shopifyOrderId = @shopifyOrderId";
            return _connection
                    .Query<ShopifyOrderLineItem>(query, new { ShopId, shopifyOrderId })
                    .ToList();
        }

        public virtual void InsertOrderLineItem(ShopifyOrderLineItem lineitem)
        {
            lineitem.ShopId = ShopId.Value;
            var query =
                @"INSERT INTO shopifyorderlineitem
                VALUES ( @ShopId, @ShopifyOrderLineId, @ShopifyOrderId, @ProductId, @VariantId, @ReportedSku, @Quantity, @UnitPrice, @TotalDiscount )";
            _connection.Execute(query, lineitem);
        }
    }
}

