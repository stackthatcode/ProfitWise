using System;
using System.Collections.Generic;
using System.Data;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Preferences;
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

        public const int OrderLineEntry = 1;
        public const int RefundEntry = 2;
        public const int AdjustmentEntry = 3;


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
        public void RefreshReportEntryData(EntryRefreshContext context = null)
        {
            var query =
                RefreshDeleteQuery(context) +
                RefreshInsertLineItemQuery(context) +
                RefreshInsertRefundQuery(context) +
                RefreshInsertAdjustmentQuery(context);
            
            _connectionWrapper.Execute(query, 
                new { PwShopId, context.ShopifyOrderId, OrderLineEntry, AdjustmentEntry, RefundEntry });
        }
        
        private readonly string PaymentStatusFieldExpr =
            $@"CASE WHEN FinancialStatus IN (3, 4, 5, 6) THEN {PaymentStatus.Captured} 
            ELSE {PaymentStatus.NotCaptured} END AS PaymentStatus ";

        private string OrderIdWhereClause(string alias = null)
        {
            var prefix = alias != null ? $"{alias}." : "";
            return $"WHERE {prefix}ShopifyOrderId = @ShopifyOrderId ";
        }

        private string RefreshInsertLineItemQuery(EntryRefreshContext context)
        {
            var query =
                @"INSERT INTO profitreportentry(@PwShopId)
                SELECT 	PwShopId, OrderDate, @OrderLineEntry AS EntryType, ShopifyOrderId, ShopifyOrderLineId AS SourceId, 
		                PwProductId, PwVariantId, TotalAfterAllDiscounts AS NetSales, 
                        Quantity * ISNULL(UnitCogs, 0) AS CoGS,
                        Quantity AS Quantity, " + 
                        PaymentStatusFieldExpr + 
                        @" FROM orderlineitem(@PwShopId) ";

            if (context.ShopifyOrderId != null)
                query += OrderIdWhereClause();
            return query + "; ";
        }

        private string RefreshInsertRefundQuery(EntryRefreshContext context)
        {
            var query =
                @"INSERT INTO profitreportentry(@PwShopId)
                SELECT 	t1.PwShopId, t1.RefundDate, @RefundEntry AS EntryType, t1.ShopifyOrderId, t1.ShopifyRefundId AS SourceId, 
		                t1.PwProductId, t1.PwVariantId, -t1.Amount AS NetSales, 	
                        -t1.RestockQuantity * ISNULL(UnitPrice, 0) AS CoGS, 	
	                    -t1.RestockQuantity AS Quantity, " +
                        PaymentStatusFieldExpr + 
                @"FROM orderrefund(@PwShopId) t1
		            INNER JOIN orderlineitem(@PwShopId) t2
			            ON t1.ShopifyOrderId = t2.ShopifyOrderId AND t1.ShopifyOrderLineId = t2.ShopifyOrderLineId ";

            if (context.ShopifyOrderId != null)
                query += OrderIdWhereClause("t1");
            return query + "; ";
        }
        
        private string RefreshInsertAdjustmentQuery(EntryRefreshContext context)
        {
            var query =
                @"INSERT INTO profitreportentry(@PwShopId)
                SELECT t1.PwShopId, t1.AdjustmentDate, @AdjustmentEntry AS EntryType, t1.ShopifyOrderId, 
                    t1.ShopifyAdjustmentId AS SourceId, NULL, NULL, t1.Amount AS NetSales, 
                    0 AS CoGS, NULL AS Quantity, " + PaymentStatusFieldExpr + 
                @"FROM orderadjustment(@PwShopId) t1 
                    INNER JOIN ordertable(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId ";

            if (context.ShopifyOrderId != null)
                query += OrderIdWhereClause("t1");
            return query + "; ";
        }

        private string RefreshDeleteQuery(EntryRefreshContext context)
        {
            var query = @"DELETE FROM profitreportentry(@PwShopId) ";

            if (context.ShopifyOrderId != null)
                query += OrderIdWhereClause();
            return query + "; ";
        }
    }
}

