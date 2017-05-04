using System;
using System.Data;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Shop;


namespace ProfitWise.Data.Repositories.Multitenant
{
    [Intercept(typeof(ShopRequired))]
    public class CogsDownstreamRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;


        public CogsDownstreamRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }
        

        // Order Line update queries
        public void UpdateOrderLines(CogsDateBlockContext lineContext)
        {
            if (lineContext.PwMasterVariantId == null && lineContext.PwMasterProductId == null)
            {
                throw new ArgumentNullException(
                    "PwMasterVariantId and PwMasterProductId can't both be null");
            }
            if (lineContext.CogsTypeId == CogsType.FixedAmount)
            {
                UpdateOrderLineFixedAmount(lineContext);
            }
            if (lineContext.CogsTypeId == CogsType.MarginPercentage)
            {
                UpdateOrderLinePercentage(lineContext);
            }
        }

        // WARNING - will set Order Line to "0" if the Exchange Rates are not up-to-date
        public void UpdateOrderLineFixedAmount(CogsDateBlockContext lineContext)
        {
            var query =
                @"UPDATE t3 SET t3.UnitCogs = (@CogsAmount * ISNULL(t4.Rate, 0))                 
                FROM mastervariant(@PwShopId) t1 
	                INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN orderlineitem(@PwShopId) t3
		                ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	           
                    LEFT JOIN exchangerate t4
		                ON t3.OrderDate = t4.[Date] 
			                AND t4.SourceCurrencyId = @CogsCurrencyId
			                AND t4.DestinationCurrencyId = @DestinationCurrencyId
                WHERE t1.PwShopId = @PwShopId " +
                WhereClauseGenerator(lineContext);

            lineContext.PwShopId = this.PwShopId;
            _connectionWrapper.Execute(query, lineContext);
        }

        public void UpdateOrderLinePercentage(CogsDateBlockContext lineContext)
        {
            var query =
                @"UPDATE t3 SET t3.UnitCogs = @CogsPercentOfUnitPrice * t3.UnitPrice / 100.00
                FROM mastervariant(@PwShopId) t1 
	                INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN orderlineitem(@PwShopId) t3
		                ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	                               
                WHERE t1.PwShopId = @PwShopId " + 
                WhereClauseGenerator(lineContext);

            lineContext.PwShopId = this.PwShopId;
            _connectionWrapper.Execute(query, lineContext);
        }

        public void UpdateOrderLinesPickList(CogsDateBlockContext lineContext)
        {
            if (lineContext.PwPickListId == null)
            {
                throw new ArgumentNullException("PwPickListId can't both be null");
            }
            if (lineContext.CogsTypeId == CogsType.FixedAmount)
            {
                UpdateOrderLineFixedAmountPickList(lineContext);
            }
            if (lineContext.CogsTypeId == CogsType.MarginPercentage)
            {
                UpdateOrderLinePercentagePickList(lineContext);
            }
        }

        public void UpdateOrderLineFixedAmountPickList(CogsDateBlockContext context)
        {
            var query =
                @"UPDATE t3
                SET t3.UnitCogs = (@CogsAmount * ISNULL(t4.Rate, 0)) 
                FROM picklistmasterproduct(@PwShopId) t0
                    INNER JOIN mastervariant(@PwShopId) t1 
                        ON t0.PwMasterProductId = t1.PwMasterProductId
	                INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN orderlineitem(@PwShopId) t3
		                ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	           
                    LEFT JOIN exchangerate t4
		                ON t3.OrderDate = t4.[Date] 
			                AND t4.SourceCurrencyId = @CogsCurrencyId
			                AND t4.DestinationCurrencyId = @DestinationCurrencyId
                WHERE t0.PwPickListId = @PwPickListId";

            context.PwShopId = this.PwShopId;
            _connectionWrapper.Execute(query, context);
        }

        public void UpdateOrderLinePercentagePickList(CogsDateBlockContext context)
        {
            var query =
                @"UPDATE t3 SET t3.UnitCogs = @CogsPercentOfUnitPrice * t3.UnitPrice / 100.00
                FROM picklistmasterproduct(@PwShopId) t0
                    INNER JOIN mastervariant(@PwShopId) t1 
                        ON t0.PwMasterProductId = t1.PwMasterProductId
	                INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN orderlineitem(@PwShopId) t3
		                ON t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
                WHERE t0.PwPickListId = @PwPickListId";

            context.PwShopId = this.PwShopId;
            _connectionWrapper.Execute(query, context);
        }

        private string WhereClauseGenerator(CogsDateBlockContext lineContext)
        {
            var output = "";
            if (lineContext.PwMasterProductId.HasValue)
            {
                output += "AND t1.PwMasterProductId = @PwMasterProductId ";
            }
            if (lineContext.PwMasterVariantId.HasValue)
            {
                output += "AND t1.PwMasterVariantId = @PwMasterVariantId ";
            }
            output += "AND t3.OrderDate >= @StartDate ";
            output += "AND t3.OrderDate <= @EndDate ";
            return output;
        }
        

        // Report Entry queries
        public void DeleteEntryLedger(EntryRefreshContext context)
        {
            var query = DeleteEntriesQuery(context);
            ExecuteLedgeQuery(context, query);
        }

        public void RefreshEntryLedger(EntryRefreshContext context)
        {
            var query = InsertLineItemEntriesQuery(context) +
                        InsertRefundEntriesQuery(context) +
                        InsertAdjustmentEntriesQuery(context);
            ExecuteLedgeQuery(context, query);
        }

        private void ExecuteLedgeQuery(EntryRefreshContext context, string query)
        {
            _connectionWrapper.Execute(query, new
            {
                PwShopId,
                context.ShopifyOrderId,
                context.PwMasterProductId,
                context.PwMasterVariantId,
                context.PwPickListId,
                EntryType.OrderLineEntry,
                EntryType.AdjustmentEntry,
                EntryType.RefundEntry,
                EntryType.CorrectionEntry
            });
        }

        private readonly string _paymentStatusInsertField =
            $@"CASE WHEN FinancialStatus IN (
                {FinancialStatus.PartiallyPaid}, {FinancialStatus.Paid}, {FinancialStatus.PartiallyRefunded}, 
                {FinancialStatus.Refunded} ) 
            THEN {PaymentStatus.Captured} ELSE {PaymentStatus.NotCaptured} END AS PaymentStatus ";

        private string OrderIdWhereClause(string alias = null, string clausePrefix = "WHERE")
        {
            var aliasWithOperator = alias != null ? $"{alias}." : "";
            return $"{clausePrefix} {aliasWithOperator}ShopifyOrderId = @ShopifyOrderId ";
        }

        private string InsertLineItemEntriesQuery(EntryRefreshContext context)
        {
            var query =
                @"INSERT INTO profitreportentry(@PwShopId)
                SELECT 	PwShopId, OrderDate, @OrderLineEntry AS EntryType, ShopifyOrderId, ShopifyOrderLineId AS SourceId, 
		                PwProductId, PwVariantId, TotalAfterAllDiscounts AS NetSales, 
                        Quantity * ISNULL(UnitCogs, 0) AS CoGS,
                        Quantity AS Quantity, " + 
                        _paymentStatusInsertField +
                        @", ShopifyOrderId FROM orderlineitem(@PwShopId) ";

            if (context.ShopifyOrderId != null)
                query += OrderIdWhereClause();
            return query + "; ";
        }

        private string InsertRefundEntriesQuery(EntryRefreshContext context)
        {
            var query =
                @"INSERT INTO profitreportentry(@PwShopId)
                SELECT 	t1.PwShopId, t1.RefundDate, @RefundEntry AS EntryType, t1.ShopifyOrderId, t1.ShopifyRefundId AS SourceId, 
		                t1.PwProductId, t1.PwVariantId, -t1.Amount AS NetSales, 	
                        -t1.RestockQuantity * ISNULL(t2.UnitCoGS, 0) AS CoGS, 	
	                    -t1.RestockQuantity AS Quantity, " +
                        _paymentStatusInsertField + @", NULL " + 
                    @" FROM orderrefund(@PwShopId) t1
		            INNER JOIN orderlineitem(@PwShopId) t2
			            ON t1.ShopifyOrderId = t2.ShopifyOrderId AND t1.ShopifyOrderLineId = t2.ShopifyOrderLineId ";

            if (context.ShopifyOrderId != null)
                query += OrderIdWhereClause("t1");
            return query + "; ";
        }
        
        private string InsertAdjustmentEntriesQuery(EntryRefreshContext context)
        {
            var query =
                @"INSERT INTO profitreportentry(@PwShopId)
                SELECT t1.PwShopId, t1.AdjustmentDate, @AdjustmentEntry AS EntryType, t1.ShopifyOrderId, 
                    t1.ShopifyAdjustmentId AS SourceId, NULL, NULL, t1.Amount AS NetSales, 
                    0 AS CoGS, NULL AS Quantity, " + _paymentStatusInsertField + @", NULL " +
                @"FROM orderadjustment(@PwShopId) t1 
                    INNER JOIN ordertable(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId ";

            if (context.ShopifyOrderId != null)
                query += OrderIdWhereClause("t1");
            return query + "; ";
        }

        [Obsolete("Should not need these corrections - provided Shopify rectifies their reporting")]
        private string InsertCorrectionEntriesQuery(EntryRefreshContext context)
        {
            var query =
                @"INSERT INTO profitreportentry(@PwShopId)
                SELECT	PwShopId, LastActivityDate, @CorrectionEntry AS EntryType, ShopifyOrderId, ShopifyOrderId, 
		                NULL, NULL, BalancingCorrection AS NetSales, 0 AS CoGS, NULL AS Quantity, " + 
                _paymentStatusInsertField +
                @" FROM ordertable(@PwShopId) o WHERE BalancingCorrection <> 0 ";

            if (context.ShopifyOrderId != null)
                query += OrderIdWhereClause("o", "AND");
            return query + "; ";
        }

        private string DeleteEntriesQuery(EntryRefreshContext context)
        {
            var query = @"DELETE FROM profitreportentry(@PwShopId) ";

            if (context.ShopifyOrderId != null)
                query += OrderIdWhereClause();
            return query + "; ";
        }


        // NOTE: this must never be called by the Order Refresh Process
        public void UpdateReportEntryLedger(EntryRefreshContext context)
        {
            context = context ?? new EntryRefreshContext();
            var query =
                UpdateLineItemEntriesQuery(context) +
                UpdateRefundEntriesQuery(context) +
                UpdateAdjustmentEntriesQuery(context);

            _connectionWrapper.Execute(query, new
                {
                    PwShopId, context.ShopifyOrderId, context.PwMasterProductId, context.PwMasterVariantId,
                    context.PwPickListId, EntryType.OrderLineEntry, EntryType.AdjustmentEntry,
                    EntryType.RefundEntry, EntryType.CorrectionEntry
                });
        }

        private readonly string PaymentStatusUpdateField =
            $@"pr.PaymentStatus = CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN {PaymentStatus.Captured} 
            ELSE {PaymentStatus.NotCaptured} END ";

        private string UpdateLineItemEntriesQuery(EntryRefreshContext context)
        {
            var query =
                @"UPDATE pr
                SET pr.CoGS = pr.Quantity * ISNULL(ol.UnitCogs, 0), " +
                PaymentStatusUpdateField + 
                @" FROM profitreportentry(@PwShopId) pr
	                INNER JOIN orderlineitem(@PwShopId) ol 
                        ON pr.ShopifyOrderId = ol.ShopifyOrderId 
                        AND pr.SourceId = ol.ShopifyOrderLineId ";
                
            return UpdateFilterAppender(query, "ol", context) + "; ";
        }

        private string UpdateRefundEntriesQuery(EntryRefreshContext context)
        {
            var query =
                @"UPDATE pr
                SET pr.CoGS = -orf.RestockQuantity * ISNULL(oli.UnitCoGS, 0), " +
                PaymentStatusUpdateField +
                @" FROM profitreportentry(@PwShopId) pr
	                INNER JOIN orderrefund(@PwShopId) orf
		                ON pr.ShopifyOrderId = orf.ShopifyOrderId 
                        AND pr.SourceId = orf.ShopifyRefundId
	                INNER JOIN orderlineitem(@PwShopId) oli
		                ON orf.ShopifyOrderId = oli.ShopifyOrderId 
                        AND orf.ShopifyOrderLineId = oli.ShopifyOrderLineId ";
            
            return UpdateFilterAppender(query, "oli", context) + "; ";
        }

        private string UpdateAdjustmentEntriesQuery(EntryRefreshContext context)
        {
            var query =
                @"UPDATE pr SET " +
                PaymentStatusUpdateField +
                @" FROM profitreportentry(@PwShopId) pr
                    INNER JOIN orderadjustment(@PwShopId) oa 
                        ON pr.ShopifyOrderId = oa.ShopifyOrderId
                        AND pr.SourceId = oa.ShopifyAdjustmentId
                    INNER JOIN ordertable(@PwShopId) o 
                        ON oa.ShopifyOrderId = o.ShopifyOrderId ";
            return query + "; ";
        }

        [Obsolete]
        private string UpdateCorrectionEntriesQuery(EntryRefreshContext context)
        {
            var query =
                @"UPDATE pr SET " + PaymentStatusUpdateField +
                $@" FROM profitreportentry(@PwShopId) pr
                    INNER JOIN ordertable(@PwShopId) o 
                        ON pr.ShopifyOrderId = o.ShopifyOrderId 
                        AND pr.EntryType = {EntryType.CorrectionEntry} ";
            return query + "; ";
        }

        private string UpdateFilterAppender(string query, string orderLineAlias, EntryRefreshContext context)
        {
            if (context.ShopifyOrderId != null)
            {
                return query + $"WHERE {orderLineAlias}.ShopifyOrderId = @ShopifyOrderId ";
            }
            if (context.PwMasterVariantId != null)
            {
                return query +
                    $@" INNER JOIN variant(@PwShopId) v ON v.PwVariantId = {orderLineAlias}.PwVariantId	
                    WHERE v.PwMasterVariantId = @PwMasterVariantId ";
            }
            if (context.PwMasterProductId != null)
            {
                return query +
                    $@" INNER JOIN variant(@PwShopId) v ON {orderLineAlias}.PwVariantId = v.PwVariantId
	                    INNER JOIN mastervariant(@PwShopId) mv ON v.PwMasterVariantId = mv.PwMasterVariantId
                    WHERE mv.PwMasterProductId = @PwMasterProductId ";
            }
            if (context.PwPickListId != null)
            {
                return query +
                    $@" INNER JOIN variant(@PwShopId) v ON {orderLineAlias}.PwVariantId = v.PwVariantId
	                INNER JOIN mastervariant(@PwShopId) mv ON v.PwMasterVariantId = mv.PwMasterVariantId
                    WHERE mv.PwMasterProductId IN ( 
                            SELECT PwMasterProductId FROM picklistmasterproduct(@PwShopId) 
                            WHERE PwPickListId = @PwPickListId ) ";
            }

            return query + "; ";
        }
    }
}

