using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Model.ShopifyImport;


namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class ShopifyOrderRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;

        public ShopifyOrderRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }

        public void CommitTransaction()
        {
            _connectionWrapper.CommitTranscation();
        }


        // Orders
        public virtual ShopifyOrder RetrieveOrders()
        {
            var query = @"SELECT * FROM shopifyorder WHERE PwShopId";
            return
                Connection
                    .Query<ShopifyOrder>(query, new { PwShopId }, _connectionWrapper.Transaction)
                    .FirstOrDefault();
        }

        public virtual ShopifyOrder RetrieveOrder(long shopifyOrderId)
        {
            var query = @"SELECT * FROM shopifyorder WHERE PwShopId = @PwShopId AND ShopifyOrderId = @shopifyOrderId";
            return Connection
                    .Query<ShopifyOrder>(
                            query, new { PwShopId = PwShopId, shopifyOrderId },
                            _connectionWrapper.Transaction)
                    .FirstOrDefault();
        }

        public virtual IList<ShopifyOrder> RetrieveOrdersFullDepth(IList<long> orderIdList)
        {
            var query = @"SELECT * FROM shopifyorder WHERE ShopifyOrderId IN @orderIdList";
            var orders = 
                Connection.Query<ShopifyOrder>(
                    query, new {orderIdList}, _connectionWrapper.Transaction).ToList();

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
            Connection.Execute(query, order, _connectionWrapper.Transaction);
        }

        public virtual void UpdateOrder(ShopifyOrder order)
        {
            order.PwShopId = PwShopId;
            var query = @"UPDATE shopifyorder SET
                                Email = @Email,
                                FinancialStatus = @FinancialStatus,
                                Tags = @Tags,
                                UpdatedAt = @UpdatedAt,
                                Cancelled = @Cancelled
                            WHERE PwShopId = @PwShopId AND ShopifyOrderId = @ShopifyOrderId";
            Connection.Execute(query, order, _connectionWrapper.Transaction);
        }

        public virtual void DeleteOrderFullDepth(long shopifyOrderId)
        {
            var query = @"DELETE FROM shopifyorder 
                        WHERE PwShopId = @PwShopId AND ShopifyOrderId = @shopifyOrderId";
            Connection.Execute(
                query, new { PwShopId = PwShopId, shopifyOrderId }, _connectionWrapper.Transaction);

            DeleteRefunds(shopifyOrderId);
            DeleteLineItems(shopifyOrderId);
            DeleteAdjustments(shopifyOrderId);
        }

        public virtual void MassDelete(IList<ShopifyOrder> orders)
        {
            var orderIdList = orders.Select(x => x.ShopifyOrderId).ToList();

            var query = @"DELETE FROM shopifyorderlineitem WHERE ShopifyOrderId IN @orderIdList";
            Connection.Execute(query, new { orderIdList }, _connectionWrapper.Transaction);

            var query2 = @"DELETE FROM shopifyorder WHERE ShopifyOrderId IN @orderIdList";
            Connection.Execute(query2, new { orderIdList }, _connectionWrapper.Transaction);
        }        



        // Line Items
        public virtual IList<ShopifyOrderLineItem> RetrieveLineItems(long shopifyOrderId)
        {
            var query = 
                @"SELECT * FROM shopifyorderlineitem WHERE PwShopId = @PwShopId AND shopifyOrderId = @shopifyOrderId";
            return Connection
                    .Query<ShopifyOrderLineItem>(
                            query, new { PwShopId = PwShopId, shopifyOrderId }, _connectionWrapper.Transaction)
                    .ToList();
        }

        public virtual IList<ShopifyOrderLineItem> RetrieveLineItems(IList<long> orderIdList)
        {
            var query = 
                @"SELECT * FROM shopifyorderlineitem WHERE PwShopId = @PwShopId AND ShopifyOrderId IN @orderIdList";
            return Connection.Query<ShopifyOrderLineItem>(query, new {PwShopId, orderIdList }, _connectionWrapper.Transaction).ToList();
        }

        public virtual void InsertLineItem(ShopifyOrderLineItem lineitem)
        {
            lineitem.PwShopId = PwShopId;
            lineitem.OrderDateTimestamp = DateTime.Now;
            var query =
                @"INSERT INTO shopifyorderlineitem ( 
                    PwShopId, ShopifyOrderId, ShopifyOrderLineId, OrderDateTimestamp, OrderDate, FinancialStatus, PwProductId, PwVariantId, 
                    Quantity, UnitPrice, TotalDiscount, TotalAfterAllDiscounts, NetQuantity, UnitCogs )
                VALUES ( 
                    @PwShopId, @ShopifyOrderId, @ShopifyOrderLineId, @OrderDateTimestamp, @OrderDate, @FinancialStatus, @PwProductId, @PwVariantId,
                    @Quantity, @UnitPrice, @TotalDiscount, @TotalAfterAllDiscounts, @NetQuantity, @UnitCogs )";
            Connection.Execute(query, lineitem, _connectionWrapper.Transaction);
        }

        public virtual void UpdateLineItemNetTotalAndStatus(ShopifyOrderLineItem lineItem)
        {
            lineItem.PwShopId = PwShopId;
            var query = @"UPDATE shopifyorderlineitem 
                            SET TotalAfterAllDiscounts = @TotalAfterAllDiscounts, 
                                NetQuantity = @NetQuantity,
                                FinancialStatus = @FinancialStatus
                            WHERE PwShopId = @PwShopId AND ShopifyOrderLineId = @ShopifyOrderLineId";
            Connection.Execute(query, lineItem, _connectionWrapper.Transaction);
        }

        public virtual void DeleteLineItems(long shopifyOrderId)
        {
            var query =
                @"DELETE FROM shopifyorderlineitem WHERE PwShopId = @PwShopId AND ShopifyOrderId = @shopifyOrderId";
            Connection.Execute(query, new { PwShopId = PwShopId, shopifyOrderId }, _connectionWrapper.Transaction);
        }



        // Refunds
        public virtual IList<ShopifyOrderLineRefund> RetrieveRefunds(long shopifyOrderId)
        {
            var query = 
                @"SELECT * FROM shopifyorderrefund WHERE PwShopId = @PwShopId AND shopifyOrderId = @shopifyOrderId";
            return Connection
                    .Query<ShopifyOrderLineRefund>(
                        query, new { PwShopId = PwShopId, shopifyOrderId }, _connectionWrapper.Transaction)
                    .ToList();
        }

        public virtual IList<ShopifyOrderLineRefund> RetrieveRefunds(IList<long> orderIdList)
        {
            var query = 
                @"SELECT * FROM shopifyorderrefund WHERE PwShopId = @PwShopId AND ShopifyOrderId IN @orderIdList";
            return Connection.Query<ShopifyOrderLineRefund>(
                    query, new { PwShopId, orderIdList }, _connectionWrapper.Transaction).ToList();
        }

        public virtual void InsertRefund(ShopifyOrderLineRefund refund)
        {
            refund.PwShopId = PwShopId;
            var query =
                @"INSERT INTO shopifyorderrefund ( 
                    PwShopId, ShopifyRefundId, ShopifyOrderId, ShopifyOrderLineId, RefundDate, 
                    PwProductId, PwVariantId, Amount, RestockQuantity )
                VALUES ( 
                    @PwShopId, @ShopifyRefundId, @ShopifyOrderId, @ShopifyOrderLineId, @RefundDate, 
                    @PwProductId, @PwVariantId, @Amount, @RestockQuantity )";
            Connection.Execute(query, refund, _connectionWrapper.Transaction);
        }

        public virtual void DeleteRefunds(long shopifyOrderId)
        {
            var query =
                @"DELETE FROM shopifyorderrefund WHERE PwShopId = @PwShopId AND ShopifyOrderId = @shopifyOrderId";
            Connection.Execute(query, new { PwShopId = PwShopId, shopifyOrderId }, _connectionWrapper.Transaction);
        }



        // Adjustments
        public virtual IList<ShopifyOrderAdjustment> RetrieveAdjustments(long shopifyOrderId)
        {
            var query =
                @"SELECT * FROM shopifyorderadjustment WHERE PwShopId = @PwShopId AND shopifyOrderId = @shopifyOrderId";
            return Connection
                    .Query<ShopifyOrderAdjustment>(
                        query, new { PwShopId = PwShopId, shopifyOrderId }, _connectionWrapper.Transaction)
                    .ToList();
        }

        public virtual IList<ShopifyOrderAdjustment> RetrieveAdjustments(IList<long> orderIdList)
        {
            var query =
                @"SELECT * FROM shopifyorderadjustment WHERE PwShopId = @PwShopId AND ShopifyOrderId IN @orderIdList";
            return Connection.Query<ShopifyOrderAdjustment>(
                query, new { PwShopId, orderIdList }, _connectionWrapper.Transaction).ToList();
        }

        public virtual void InsertAdjustment(ShopifyOrderAdjustment refund)
        {
            refund.PwShopId = PwShopId;
            var query =
                @"INSERT INTO shopifyorderadjustment ( 
                    PwShopId, ShopifyAdjustmentId, AdjustmentDate, ShopifyOrderId, Amount, 
                    TaxAmount, Kind, Reason )
                VALUES ( 
                    @PwShopId, @ShopifyAdjustmentId, @AdjustmentDate, @ShopifyOrderId, @Amount, 
                    @TaxAmount, @Kind, @Reason )";
            Connection.Execute(query, refund, _connectionWrapper.Transaction);
        }

        public virtual void DeleteAdjustments(long shopifyOrderId)
        {
            var query =
                @"DELETE FROM shopifyorderadjustment WHERE PwShopId = @PwShopId AND ShopifyOrderId = @shopifyOrderId";
            Connection.Execute(query, new { PwShopId = PwShopId, shopifyOrderId }, _connectionWrapper.Transaction);
        }



        public virtual IList<OrderLineItemSubset> RetrieveLineItemSubset()
        {
            var query = @"SELECT ShopifyOrderId, ShopifyOrderLineId, PwProductId, PwVariantId, Quantity, UnitPrice
                        FROM shopifyorderlineitem  WHERE PwShopId = @PwShopId 
                        ORDER BY ShopifyOrderId ASC, ShopifyOrderLineId ASC";
            return Connection
                    .Query<OrderLineItemSubset>(
                            query, new {PwShopId = PwShopId}, _connectionWrapper.Transaction)
                    .ToList();
        }
    }
}

