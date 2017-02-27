using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Model.ShopifyImport;

namespace ProfitWise.Data.Repositories.Multitenant
{
    [Intercept(typeof(ShopRequired))]
    public class ShopifyOrderRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;

        public ShopifyOrderRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }

        public void CommitTransaction()
        {
            _connectionWrapper.CommitTranscation();
        }


        // Orders
        public virtual ShopifyOrder RetrieveOrders()
        {
            var query = @"SELECT * FROM ordertable(@PwShopId);";
            return
                _connectionWrapper.Query<ShopifyOrder>(query, new { PwShopId }).FirstOrDefault();
        }

        public virtual ShopifyOrder RetrieveOrder(long shopifyOrderId)
        {
            var query = @"SELECT * FROM ordertable(@PwShopId) WHERE ShopifyOrderId = @shopifyOrderId";
            return _connectionWrapper
                    .Query<ShopifyOrder>(query, new { PwShopId, shopifyOrderId }).FirstOrDefault();
        }

        public virtual IList<ShopifyOrder> RetrieveOrdersFullDepth(IList<long> orderIdList)
        {
            var query = @"SELECT * FROM ordertable(@PwShopId) WHERE ShopifyOrderId IN @orderIdList";
            var orders =
                _connectionWrapper.Query<ShopifyOrder>(query, new {orderIdList}).ToList();

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
            var query = @"INSERT INTO ordertable(@PwShopId) 
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
            _connectionWrapper.Execute(query, order);
        }

        public virtual void UpdateOrder(ShopifyOrder order)
        {
            order.PwShopId = PwShopId;
            var query = @"UPDATE ordertable(@PwShopId) SET
                                Email = @Email,
                                FinancialStatus = @FinancialStatus,
                                Tags = @Tags,
                                UpdatedAt = @UpdatedAt,
                                Cancelled = @Cancelled
                            WHERE ShopifyOrderId = @ShopifyOrderId";
            _connectionWrapper.Execute(query, order);
        }

        public virtual void DeleteOrderFullDepth(long shopifyOrderId)
        {
            var query = @"DELETE FROM ordertable(@PwShopId) WHERE ShopifyOrderId = @shopifyOrderId";
            _connectionWrapper.Execute(query, new { PwShopId, shopifyOrderId });

            DeleteRefunds(shopifyOrderId);
            DeleteLineItems(shopifyOrderId);
            DeleteAdjustments(shopifyOrderId);
        }

        public virtual void MassDelete(IList<ShopifyOrder> orders)
        {
            var orderIdList = orders.Select(x => x.ShopifyOrderId).ToList();

            var query = @"DELETE FROM orderlineitem(@PwShopId) WHERE ShopifyOrderId IN @orderIdList";
            _connectionWrapper.Execute(query, new { orderIdList });

            var query2 = @"DELETE FROM ordertable(@PwShopId) WHERE ShopifyOrderId IN @orderIdList";
            _connectionWrapper.Execute(query2, new { orderIdList });
        }        



        // Line Items
        public virtual IList<ShopifyOrderLineItem> RetrieveLineItems(long shopifyOrderId)
        {
            var query = @"SELECT * FROM orderlineitem(@PwShopId) WHERE shopifyOrderId = @shopifyOrderId;";
            return _connectionWrapper
                    .Query<ShopifyOrderLineItem>(
                            query, new { PwShopId = PwShopId, shopifyOrderId })
                    .ToList();
        }

        public virtual IList<ShopifyOrderLineItem> RetrieveLineItems(IList<long> orderIdList)
        {
            var query = 
                @"SELECT * FROM orderlineitem(@PwShopId) WHERE ShopifyOrderId IN @orderIdList;";
            return _connectionWrapper.Query<ShopifyOrderLineItem>(query, new {PwShopId, orderIdList }).ToList();
        }

        public virtual void InsertLineItem(ShopifyOrderLineItem lineitem)
        {
            lineitem.PwShopId = PwShopId;
            lineitem.OrderDateTimestamp = DateTime.Now;
            var query =
                @"INSERT INTO orderlineitem(@PwShopId) ( 
                    PwShopId, ShopifyOrderId, ShopifyOrderLineId, OrderDateTimestamp, OrderDate, FinancialStatus, PwProductId, PwVariantId, 
                    Quantity, UnitPrice, TotalDiscount, TotalAfterAllDiscounts, NetQuantity, UnitCogs )
                VALUES ( 
                    @PwShopId, @ShopifyOrderId, @ShopifyOrderLineId, @OrderDateTimestamp, @OrderDate, @FinancialStatus, @PwProductId, @PwVariantId,
                    @Quantity, @UnitPrice, @TotalDiscount, @TotalAfterAllDiscounts, @NetQuantity, @UnitCogs )";
            _connectionWrapper.Execute(query, lineitem);
        }

        public virtual void UpdateLineItemNetTotalAndStatus(ShopifyOrderLineItem lineItem)
        {
            lineItem.PwShopId = PwShopId;
            var query = @"UPDATE orderlineitem(@PwShopId) 
                            SET TotalAfterAllDiscounts = @TotalAfterAllDiscounts, 
                                NetQuantity = @NetQuantity,
                                FinancialStatus = @FinancialStatus
                            WHERE ShopifyOrderLineId = @ShopifyOrderLineId";
            _connectionWrapper.Execute(query, lineItem);
        }

        public virtual void DeleteLineItems(long shopifyOrderId)
        {
            var query = @"DELETE FROM orderlineitem(@PwShopId) WHERE ShopifyOrderId = @shopifyOrderId";
            _connectionWrapper.Execute(query, new { PwShopId = PwShopId, shopifyOrderId });
        }



        // Refunds
        public virtual IList<ShopifyOrderLineRefund> RetrieveRefunds(long shopifyOrderId)
        {
            var query = @"SELECT * FROM orderrefund(@PwShopId) WHERE shopifyOrderId = @shopifyOrderId;";
            return _connectionWrapper
                    .Query<ShopifyOrderLineRefund>(query, new { PwShopId, shopifyOrderId })
                    .ToList();
        }

        public virtual IList<ShopifyOrderLineRefund> RetrieveRefunds(IList<long> orderIdList)
        {
            var query = 
                @"SELECT * FROM orderrefund(@PwShopId) WHERE ShopifyOrderId IN @orderIdList";
            return _connectionWrapper.Query<ShopifyOrderLineRefund>(
                    query, new { PwShopId, orderIdList }).ToList();
        }

        public virtual void InsertRefund(ShopifyOrderLineRefund refund)
        {
            refund.PwShopId = PwShopId;
            var query =
                @"INSERT INTO orderrefund(@PwShopId) ( 
                    PwShopId, ShopifyRefundId, ShopifyOrderId, ShopifyOrderLineId, RefundDate, 
                    PwProductId, PwVariantId, Amount, RestockQuantity )
                VALUES ( 
                    @PwShopId, @ShopifyRefundId, @ShopifyOrderId, @ShopifyOrderLineId, @RefundDate, 
                    @PwProductId, @PwVariantId, @Amount, @RestockQuantity )";
            _connectionWrapper.Execute(query, refund);
        }

        public virtual void DeleteRefunds(long shopifyOrderId)
        {
            var query = @"DELETE FROM orderrefund(@PwShopId) WHERE ShopifyOrderId = @shopifyOrderId";
            _connectionWrapper.Execute(query, new { PwShopId, shopifyOrderId });
        }



        // Adjustments
        public virtual IList<ShopifyOrderAdjustment> RetrieveAdjustments(long shopifyOrderId)
        {
            var query = @"SELECT * FROM orderadjustment(@PwShopId) WHERE shopifyOrderId = @shopifyOrderId";
            return _connectionWrapper
                    .Query<ShopifyOrderAdjustment>(query, new { PwShopId, shopifyOrderId })
                    .ToList();
        }

        public virtual IList<ShopifyOrderAdjustment> RetrieveAdjustments(IList<long> orderIdList)
        {
            var query =
                @"SELECT * FROM orderadjustment(@PwShopId) WHERE ShopifyOrderId IN @orderIdList";
            return _connectionWrapper.Query<ShopifyOrderAdjustment>(
                query, new { PwShopId, orderIdList }).ToList();
        }

        public virtual void InsertAdjustment(ShopifyOrderAdjustment refund)
        {
            refund.PwShopId = PwShopId;
            var query =
                @"INSERT INTO orderadjustment(@PwShopId) ( 
                    PwShopId, ShopifyAdjustmentId, AdjustmentDate, ShopifyOrderId, Amount, 
                    TaxAmount, Kind, Reason )
                VALUES ( 
                    @PwShopId, @ShopifyAdjustmentId, @AdjustmentDate, @ShopifyOrderId, @Amount, 
                    @TaxAmount, @Kind, @Reason )";
            _connectionWrapper.Execute(query, refund);
        }

        public virtual void DeleteAdjustments(long shopifyOrderId)
        {
            var query = @"DELETE FROM orderadjustment(@PwShopId) WHERE ShopifyOrderId = @shopifyOrderId;";
            _connectionWrapper.Execute(query, new {PwShopId, shopifyOrderId});
        }



        public virtual IList<OrderLineItemSubset> RetrieveLineItemSubset()
        {
            var query = @"SELECT ShopifyOrderId, ShopifyOrderLineId, PwProductId, PwVariantId, Quantity, UnitPrice
                        FROM orderlineitem(@PwShopId) 
                        ORDER BY ShopifyOrderId ASC, ShopifyOrderLineId ASC";
            return _connectionWrapper.Query<OrderLineItemSubset>(query, new { PwShopId}).ToList();
        }
    }
}

