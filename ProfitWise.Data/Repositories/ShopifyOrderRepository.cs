using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class ShopifyOrderRepository : IShopFilter
    {
        private readonly MySqlConnection _connection;

        public ShopifyOrderRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public PwShop PwShop { get; set; }


        public MySqlTransaction InitiateTransaction()
        {
            return _connection.BeginTransaction();
        }


        public virtual ShopifyOrder RetrieveOrders()
        {
            var query = @"SELECT * FROM shopifyorder WHERE PwShopId";
            return
                _connection
                    .Query<ShopifyOrder>(query, new { PwShop.PwShopId })
                    .FirstOrDefault();
        }

        public virtual ShopifyOrder RetrieveOrder(long shopifyOrderId)
        {
            var query = @"SELECT * FROM shopifyorder WHERE PwShopId = @PwShopId AND ShopifyOrderId = @shopifyOrderId";
            return _connection
                    .Query<ShopifyOrder>(query, new { PwShopId = PwShop.PwShopId, shopifyOrderId })
                    .FirstOrDefault();
        }

        public virtual IList<ShopifyOrder> RetrieveOrders(IList<long> orderIdList)
        {
            var query2 = @"SELECT * FROM shopifyorder WHERE ShopifyOrderId IN @orderIdList";
            return _connection.Query<ShopifyOrder>(query2, new {orderIdList}).ToList();
        }

        public virtual void InsertOrder(ShopifyOrder order)
        {
            order.PwShopId = PwShop.PwShopId;
            var query = @"INSERT INTO shopifyorder 
                        VALUES( @ShopId, 
                                @ShopifyOrderId,
                                @Email, 
                                @OrderNumber, 
                                @OrderLevelDiscount,
                                @SubTotal, 
                                @TotalRefund, 
                                @TaxRefundAmount, 
                                @ShippingRefundAmount, 
                                @FinancialStatus,
                                @Tags,
                                @CreatedAt, 
                                @UpdatedAt)";
            _connection.Execute(query, order);
        }

        public virtual void UpdateOrder(ShopifyOrder order)
        {
            order.PwShopId = PwShop.PwShopId;
            var query = @"UPDATE shopifyorder SET
                                Email = @Email,
                                TotalRefund = @TotalRefund,
                                TaxRefundAmount = @TaxRefundAmount,
                                ShippingRefundAmount = @ShippingRefundAmount,
                                FinancialStatus = @FinancialStatus,
                                Tags = @Tags,
                                UpdatedAt = @UpdatedAt
                            WHERE PwShopId = @PwShopId AND ShopifyOrderId = @ShopifyOrderId";
            _connection.Execute(query, order);
        }

        public virtual void DeleteOrder(long shopifyOrderId)
        {
            var query = @"DELETE FROM shopifyorder 
                        WHERE ShopId = @ShopId AND ShopifyOrderId = @shopifyOrderId";
            _connection.Execute(query, new { ShopId = PwShop.PwShopId, shopifyOrderId });
        }


        public virtual void MassDelete(IList<ShopifyOrder> orders)
        {
            var orderIdList = orders.Select(x => x.ShopifyOrderId).ToList();

            var query = @"DELETE FROM shopifyorderlineitem WHERE ShopifyOrderId IN @orderIdList";
            _connection.Execute(query, new { orderIdList });

            var query2 = @"DELETE FROM shopifyorder WHERE ShopifyOrderId IN @orderIdList";
            _connection.Execute(query2, new { orderIdList });
        }



        public virtual IList<ShopifyOrderLineItem> RetrieveOrderLineItems(long shopifyOrderId)
        {
            var query = @"SELECT * FROM shopifyorderlineitem WHERE PwShopId = @ShopId AND shopifyOrderId = @shopifyOrderId";
            return _connection
                    .Query<ShopifyOrderLineItem>(query, new { PwShopId = PwShop.PwShopId, shopifyOrderId })
                    .ToList();
        }

        public virtual IList<ShopifyOrderLineItem> RetrieveOrderLineItems(IList<long> orderIdList)
        {
            var query = @"SELECT * FROM shopifyorderlineitem WHERE ShopifyOrderId IN @orderIdList";
            return _connection.Query<ShopifyOrderLineItem>(query, new {orderIdList}).ToList();
        }

        public virtual void InsertOrderLineItem(ShopifyOrderLineItem lineitem)
        {
            lineitem.ShopId = PwShop.PwShopId;
            var query =
                @"INSERT INTO shopifyorderlineitem ( 
                    PwShopId, ShopifyOrderId, ShopifyOrderLineId, OrderDate, ShopifyProductId, ShopifyVariantId, Sku, Quantity, UnitPrice, 
                    TotalDiscount, TotalRestockedQuantity, GrossRevenue, ProductTitle, VariantTitle, Name, PwProductId)
                VALUES ( 
                    @PwShopId, @ShopifyOrderId, @ShopifyOrderLineId, @OrderDate, @ShopifyProductId, @ShopifyVariantId, @Sku, @Quantity, @UnitPrice, 
                    @TotalDiscount, @TotalRestockedQuantity, @GrossRevenue, @ProductTitle, @VariantTitle, @Name, @PwProductId)";
            _connection.Execute(query, lineitem);
        }

        public virtual void UpdateOrderLineItem(ShopifyOrderLineItem lineitem)
        {
            lineitem.ShopId = PwShop.PwShopId;
            var query =
                @"UPDATE shopifyorderlineitem SET
                    ShopifyProductId = @ShopifyProductId, 
                    ShopifyVariantId = @ShopifyVariantId,
                    TotalRestockedQuantity = @TotalRestockedQuantity,
                    GrossRevenue = @GrossRevenue
                WHERE PwShopId = @PwShopId 
                AND ShopifyOrderId = @ShopifyOrderId
                AND ShopifyOrderLineId = @ShopifyOrderLineId";
            _connection.Execute(query, lineitem);
        }

        public virtual void DeleteOrderLineItems(long shopifyOrderId)
        {
            var query =
                @"DELETE FROM shopifyorderlineitem WHERE PwShopId = @ShopId AND ShopifyOrderId = @shopifyOrderId";
            _connection.Execute(query, new { PwShopId = PwShop.PwShopId, shopifyOrderId });
        }
    }
}

