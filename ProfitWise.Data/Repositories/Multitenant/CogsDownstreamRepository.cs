using System;
using System.Data;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Preferences;
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
        public void RefreshReportEntryData()
        {
            var query =
                RefreshDeleteQuery() +
                RefreshInsertLineItemQuery() +
                RefreshInsertRefundQuery() +
                RefreshInsertAdjustmentQuery();

            _connectionWrapper.Execute(
                query, new { PwShopId, PwShop.DefaultCogsPercent, OrderLineEntry, AdjustmentEntry, RefundEntry });
        }

        private string RefreshInsertLineItemQuery()
        {
            var orderQuery =
                @"INSERT INTO profitreportentry(@PwShopId)
                    SELECT 	PwShopId, OrderDate, @OrderLineEntry AS EntryType, ShopifyOrderId, ShopifyOrderLineId AS SourceId, 
		                    PwProductId, PwVariantId, TotalAfterAllDiscounts AS NetSales, ";
            if (PwShop.UseDefaultMargin)
            {
                orderQuery +=
                    @"CASE WHEN (UnitCogs = 0 OR UnitCogs IS NULL) THEN Quantity * UnitPrice * @DefaultCogsPercent 
                    ELSE Quantity * UnitCogs END AS CoGS, ";
            }
            else
            {
                orderQuery += "(Quantity * UnitCogs) AS CoGS, ";
            }

            orderQuery += @"Quantity AS Quantity FROM orderlineitem(@PwShopId) ";

            if (PwShop.ProfitRealization == ProfitRealization.PaymentClears)
            {
                orderQuery += "WHERE FinancialStatus IN ( 3, 4, 5, 6 ) ";
            }
            return orderQuery + "; ";
        }

        private string RefreshInsertRefundQuery()
        {
            var refundQuery =
                @"INSERT INTO profitreportentry(@PwShopId)
                    SELECT 	t1.PwShopId, t1.RefundDate, @RefundEntry AS EntryType, t1.ShopifyOrderId, t1.ShopifyRefundId AS SourceId, 
		                    t1.PwProductId, t1.PwVariantId, -t1.Amount AS NetSales, ";

            if (PwShop.UseDefaultMargin)
            {
                refundQuery +=
                    @"CASE WHEN (UnitCogs = 0 OR UnitCogs IS NULL) THEN -t1.RestockQuantity * UnitPrice * @DefaultCogsPercent
                    ELSE -t1.RestockQuantity * UnitCogs END AS CoGS, ";
            }
            else
            {
                refundQuery += "(-t1.RestockQuantity * UnitCogs) AS CoGS, ";
            }

            refundQuery += @"-t1.RestockQuantity AS Quantity 
                        FROM orderrefund(@PwShopId) t1
		                INNER JOIN orderlineitem(@PwShopId) t2
			                ON t1.ShopifyOrderId = t2.ShopifyOrderId 
                            AND t1.ShopifyOrderLineId = t2.ShopifyOrderLineId ";

            if (PwShop.ProfitRealization == ProfitRealization.PaymentClears)
            {
                refundQuery += "WHERE t2.FinancialStatus IN ( 3, 4, 5, 6 ) ";
            }
            return refundQuery + "; ";
        }

        private string RefreshInsertAdjustmentQuery()
        {
            var adjustmentQuery =
                @"INSERT INTO profitreportentry(@PwShopId)
                SELECT t1.PwShopId, t1.AdjustmentDate, @AdjustmentEntry AS EntryType, t1.ShopifyOrderId, 
                    t1.ShopifyAdjustmentId AS SourceId, NULL, NULL, t1.Amount AS NetSales, 0 AS CoGS, NULL AS Quantity
                FROM orderadjustment(@PwShopId) t1 
                    INNER JOIN ordertable(@PwShopId) t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId ";

            if (PwShop.ProfitRealization == ProfitRealization.PaymentClears)
            {
                adjustmentQuery += "WHERE t2.FinancialStatus IN ( 3, 4, 5, 6 ) ";
            }
            return adjustmentQuery + "; ";
        }

        private string RefreshDeleteQuery()
        {
            var deleteQuery = @"DELETE FROM profitreportentry(@PwShopId);";
            return deleteQuery;
        }
    }
}

