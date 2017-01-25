using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Model.ShopifyImport;


namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class ShopifyOrderRepository : IShopFilter
    {
        private readonly IDbConnection _connection;

        public ShopifyOrderRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public PwShop PwShop { get; set; }


        public IDbTransaction InitiateTransaction()
        {
            return _connection.BeginTransaction();
        }


        // Orders
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

        public virtual IList<ShopifyOrder> RetrieveOrdersFullDepth(IList<long> orderIdList)
        {
            var query = @"SELECT * FROM shopifyorder WHERE ShopifyOrderId IN @orderIdList";
            var orders = _connection.Query<ShopifyOrder>(query, new {orderIdList}).ToList();

            var lineItems = this.RetrieveLineItems(orderIdList);
            var refunds = this.RetrieveRefunds(orderIdList);
            var adjustments = this.RetrieveAdjustments(orderIdList);

            orders.AppendLineItems(lineItems);
            orders.AppendRefunds(refunds);
            orders.AppendAdjustments(adjustments);
            return orders;
        }

        public virtual void InsertOrder(ShopifyOrder order)
        {
            order.PwShopId = PwShop.PwShopId;
            var query = @"INSERT INTO shopifyorder 
                        VALUES( @PwShopId, 
                                @ShopifyOrderId,
                                @Email, 
                                @OrderNumber, 
                                @OrderDate,
                                @OrderLevelDiscount,
                                @FinancialStatus,
                                @Tags,
                                @CreatedAt, 
                                @UpdatedAt,
                                @Cancelled )";
            _connection.Execute(query, order);
        }

        public virtual void UpdateOrder(ShopifyOrder order)
        {
            order.PwShopId = PwShop.PwShopId;
            var query = @"UPDATE shopifyorder SET
                                Email = @Email,
                                FinancialStatus = @FinancialStatus,
                                Tags = @Tags,
                                UpdatedAt = @UpdatedAt,
                                Cancelled = @Cancelled
                            WHERE PwShopId = @PwShopId AND ShopifyOrderId = @ShopifyOrderId";
            _connection.Execute(query, order);
        }

        public virtual void DeleteOrderFullDepth(long shopifyOrderId)
        {
            var query = @"DELETE FROM shopifyorder 
                        WHERE PwShopId = @PwShopId AND ShopifyOrderId = @shopifyOrderId";
            _connection.Execute(query, new { PwShopId = PwShop.PwShopId, shopifyOrderId });

            DeleteRefunds(shopifyOrderId);
            DeleteLineItems(shopifyOrderId);
            DeleteAdjustments(shopifyOrderId);
        }

        public virtual void MassDelete(IList<ShopifyOrder> orders)
        {
            var orderIdList = orders.Select(x => x.ShopifyOrderId).ToList();

            var query = @"DELETE FROM shopifyorderlineitem WHERE ShopifyOrderId IN @orderIdList";
            _connection.Execute(query, new { orderIdList });

            var query2 = @"DELETE FROM shopifyorder WHERE ShopifyOrderId IN @orderIdList";
            _connection.Execute(query2, new { orderIdList });
        }        



        // Line Items
        public virtual IList<ShopifyOrderLineItem> RetrieveLineItems(long shopifyOrderId)
        {
            var query = 
                @"SELECT * FROM shopifyorderlineitem WHERE PwShopId = @PwShopId AND shopifyOrderId = @shopifyOrderId";
            return _connection
                    .Query<ShopifyOrderLineItem>(query, new { PwShopId = PwShop.PwShopId, shopifyOrderId })
                    .ToList();
        }

        public virtual IList<ShopifyOrderLineItem> RetrieveLineItems(IList<long> orderIdList)
        {
            var query = 
                @"SELECT * FROM shopifyorderlineitem WHERE PwShopId = @PwShopId AND ShopifyOrderId IN @orderIdList";
            return _connection.Query<ShopifyOrderLineItem>(query, new {PwShop.PwShopId, orderIdList }).ToList();
        }

        public virtual void InsertLineItem(ShopifyOrderLineItem lineitem)
        {
            lineitem.PwShopId = PwShop.PwShopId;
            var query =
                @"INSERT INTO shopifyorderlineitem ( 
                    PwShopId, ShopifyOrderId, ShopifyOrderLineId, OrderDate, FinancialStatus, PwProductId, PwVariantId, 
                    Quantity, UnitPrice, TotalDiscount, TotalAfterAllDiscounts, NetQuantity, UnitCogs )
                VALUES ( 
                    @PwShopId, @ShopifyOrderId, @ShopifyOrderLineId, @OrderDate, @FinancialStatus, @PwProductId, @PwVariantId,
                    @Quantity, @UnitPrice, @TotalDiscount, @TotalAfterAllDiscounts, @NetQuantity, @UnitCogs )";
            _connection.Execute(query, lineitem);
        }

        public virtual void UpdateLineItemNetTotalAndStatus(ShopifyOrderLineItem lineItem)
        {
            lineItem.PwShopId = PwShop.PwShopId;
            var query = @"UPDATE shopifyorderlineitem 
                            SET TotalAfterAllDiscounts = @TotalAfterAllDiscounts, 
                                NetQuantity = @NetQuantity,
                                FinancialStatus = @FinancialStatus
                            WHERE PwShopId = @PwShopId AND ShopifyOrderLineId = @ShopifyOrderLineId";
            _connection.Execute(query, lineItem);
        }

        public virtual void DeleteLineItems(long shopifyOrderId)
        {
            var query =
                @"DELETE FROM shopifyorderlineitem WHERE PwShopId = @PwShopId AND ShopifyOrderId = @shopifyOrderId";
            _connection.Execute(query, new { PwShopId = PwShop.PwShopId, shopifyOrderId });
        }



        // Refunds
        public virtual IList<ShopifyOrderLineRefund> RetrieveRefunds(long shopifyOrderId)
        {
            var query = 
                @"SELECT * FROM shopifyorderrefund WHERE PwShopId = @PwShopId AND shopifyOrderId = @shopifyOrderId";
            return _connection
                    .Query<ShopifyOrderLineRefund>(query, new { PwShopId = PwShop.PwShopId, shopifyOrderId })
                    .ToList();
        }

        public virtual IList<ShopifyOrderLineRefund> RetrieveRefunds(IList<long> orderIdList)
        {
            var query = 
                @"SELECT * FROM shopifyorderrefund WHERE PwShopId = @PwShopId AND ShopifyOrderId IN @orderIdList";
            return _connection.Query<ShopifyOrderLineRefund>(query, new { PwShop.PwShopId, orderIdList }).ToList();
        }

        public virtual void InsertRefund(ShopifyOrderLineRefund refund)
        {
            refund.PwShopId = PwShop.PwShopId;
            var query =
                @"INSERT INTO shopifyorderrefund ( 
                    PwShopId, ShopifyRefundId, ShopifyOrderId, ShopifyOrderLineId, RefundDate, 
                    PwProductId, PwVariantId, Amount, RestockQuantity )
                VALUES ( 
                    @PwShopId, @ShopifyRefundId, @ShopifyOrderId, @ShopifyOrderLineId, @RefundDate, 
                    @PwProductId, @PwVariantId, @Amount, @RestockQuantity )";
            _connection.Execute(query, refund);
        }

        public virtual void DeleteRefunds(long shopifyOrderId)
        {
            var query =
                @"DELETE FROM shopifyorderrefund WHERE PwShopId = @PwShopId AND ShopifyOrderId = @shopifyOrderId";
            _connection.Execute(query, new { PwShopId = PwShop.PwShopId, shopifyOrderId });
        }



        // Adjustments
        public virtual IList<ShopifyOrderAdjustment> RetrieveAdjustments(long shopifyOrderId)
        {
            var query =
                @"SELECT * FROM shopifyorderadjustment WHERE PwShopId = @PwShopId AND shopifyOrderId = @shopifyOrderId";
            return _connection
                    .Query<ShopifyOrderAdjustment>(query, new { PwShopId = PwShop.PwShopId, shopifyOrderId })
                    .ToList();
        }

        public virtual IList<ShopifyOrderAdjustment> RetrieveAdjustments(IList<long> orderIdList)
        {
            var query =
                @"SELECT * FROM shopifyorderadjustment WHERE PwShopId = @PwShopId AND ShopifyOrderId IN @orderIdList";
            return _connection.Query<ShopifyOrderAdjustment>(query, new { PwShop.PwShopId, orderIdList }).ToList();
        }

        public virtual void InsertAdjustment(ShopifyOrderAdjustment refund)
        {
            refund.PwShopId = PwShop.PwShopId;
            var query =
                @"INSERT INTO shopifyorderadjustment ( 
                    PwShopId, ShopifyAdjustmentId, AdjustmentDate, ShopifyOrderId, Amount, 
                    TaxAmount, Kind, Reason )
                VALUES ( 
                    @PwShopId, @ShopifyAdjustmentId, @AdjustmentDate, @ShopifyOrderId, @Amount, 
                    @TaxAmount, @Kind, @Reason )";
            _connection.Execute(query, refund);
        }

        public virtual void DeleteAdjustments(long shopifyOrderId)
        {
            var query =
                @"DELETE FROM shopifyorderadjustment WHERE PwShopId = @PwShopId AND ShopifyOrderId = @shopifyOrderId";
            _connection.Execute(query, new { PwShopId = PwShop.PwShopId, shopifyOrderId });
        }



        public virtual IList<OrderLineItemSubset> RetrieveLineItemSubset()
        {
            var query = @"SELECT ShopifyOrderId, ShopifyOrderLineId, PwProductId, PwVariantId, Quantity, UnitPrice
                        FROM shopifyorderlineitem  WHERE PwShopId = @PwShopId 
                        ORDER BY ShopifyOrderId ASC, ShopifyOrderLineId ASC";
            return _connection
                    .Query<OrderLineItemSubset>(
                            query, new {PwShopId = PwShop.PwShopId})
                    .ToList();
        }
    }
}

