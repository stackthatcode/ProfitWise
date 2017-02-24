using System;
using System.Data;
using Autofac.Extras.DynamicProxy2;
using Dapper;
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
        private IDbConnection Connection => _connectionWrapper.DbConn;


        public CogsDownstreamRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
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
                FROM profitwisemastervariant t1 
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	           
                    LEFT JOIN exchangerate t4
		                ON t3.OrderDate = t4.[Date] 
			                AND t4.SourceCurrencyId = @CogsCurrencyId
			                AND t4.DestinationCurrencyId = @DestinationCurrencyId
                WHERE t1.PwShopId = @PwShopId " +
                WhereClauseGenerator(lineContext);

            Connection.Execute(query, lineContext, _connectionWrapper.Transaction);
        }

        public void UpdateOrderLinePercentage(CogsDateBlockContext lineContext)
        {
            var query =
                @"UPDATE t3 
                SET t3.UnitCogs = @CogsPercentOfUnitPrice * t3.UnitPrice / 100.00
                FROM profitwisemastervariant t1 
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	                               
                WHERE t1.PwShopId = @PwShopId " + 
                WhereClauseGenerator(lineContext);

            Connection.Execute(query, lineContext, _connectionWrapper.Transaction);
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
                FROM profitwisepicklistmasterproduct t0
                    INNER JOIN profitwisemastervariant t1 
                        ON t0.PwShopId = t1.PwShopId AND t0.PwMasterProductId = t1.PwMasterProductId
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	           
                    LEFT JOIN exchangerate t4
		                ON t3.OrderDate = t4.[Date] 
			                AND t4.SourceCurrencyId = @CogsCurrencyId
			                AND t4.DestinationCurrencyId = @DestinationCurrencyId
                WHERE t1.PwShopId = @PwShopId AND t0.PwPickListId = @PwPickListId";
                
            Connection.Execute(query, context, _connectionWrapper.Transaction);
        }

        public void UpdateOrderLinePercentagePickList(CogsDateBlockContext context)
        {
            var query =
                @"UPDATE t3 SET t3.UnitCogs = @CogsPercentOfUnitPrice * t3.UnitPrice / 100.00
                FROM profitwisepicklistmasterproduct t0
                    INNER JOIN profitwisemastervariant t1 
                        ON t0.PwShopId = t1.PwShopId AND t0.PwMasterProductId = t1.PwMasterProductId
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
                WHERE t1.PwShopId = @PwShopId AND t0.PwPickListId = @PwPickListId";

            Connection.Execute(query, context, _connectionWrapper.Transaction);
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

            Connection.Execute(query, 
                new { PwShopId, DefaultCogsPercent = PwShop.DefaultCogsPercent }, _connectionWrapper.Transaction);
        }

        private string RefreshInsertLineItemQuery()
        {
            var orderQuery =
                @"INSERT INTO profitwiseprofitreportentry
                    SELECT 	PwShopId, OrderDate, 1 AS EntryType, ShopifyOrderId, ShopifyOrderLineId AS SourceId, 
		                    PwProductId, PwVariantId, TotalAfterAllDiscounts AS NetSales, ";
            if (PwShop.UseDefaultMargin)
            {
                orderQuery +=
                    @"CASE WHEN (UnitCogs = 0 OR UnitCogs IS NULL) THEN Quantity * UnitPrice * @DefaultCogsPercent 
                    ELSE Quantity * UnitCogs END AS CoGS, ";

                // MySQL "IF (UnitCogs = 0 OR UnitCogs IS NULL,  Quantity * UnitPrice * @DefaultCogsPercent , Quantity * UnitCogs) AS CoGS, ";

            }
            else
            {
                orderQuery += "(Quantity * UnitCogs) AS CoGS, ";
            }
            orderQuery += @"Quantity AS Quantity FROM shopifyorderlineitem ";
            if (PwShop.ProfitRealization == ProfitRealization.PaymentClears)
            {
                orderQuery += "WHERE PwShopId = @PwShopId AND FinancialStatus IN ( 3, 4, 5, 6 ); ";
            }
            else
            {
                orderQuery += @"WHERE PwShopId = @PwShopId; ";
            }
            return orderQuery;
        }

        private string RefreshInsertRefundQuery()
        {
            var refundQuery =
                @"INSERT INTO profitwiseprofitreportentry
                    SELECT 	t1.PwShopId, t1.RefundDate, 2 AS EntryType, t1.ShopifyOrderId, t1.ShopifyRefundId AS SourceId, 
		                    t1.PwProductId, t1.PwVariantId, -t1.Amount AS NetSales, ";
            if (PwShop.UseDefaultMargin)
            {
                refundQuery +=
                    @"CASE WHEN (UnitCogs = 0 OR UnitCogs IS NULL) THEN -t1.RestockQuantity * UnitPrice * @DefaultCogsPercent
                    ELSE -t1.RestockQuantity * UnitCogs END AS CoGS, ";

                // MySQL "IF (UnitCogs = 0 OR UnitCogs IS NULL, -t1.RestockQuantity * UnitPrice * @DefaultCogsPercent , -t1.RestockQuantity * UnitCogs) AS CoGS, ";
            }
            else
            {
                refundQuery += "(-t1.RestockQuantity * UnitCogs) AS CoGS, ";
            }
            refundQuery += @"-t1.RestockQuantity AS Quantity 
                        FROM shopifyorderrefund t1
		                        INNER JOIN shopifyorderlineitem t2
			                        ON t1.PwShopId = t2.PwShopId 
                                    AND t1.ShopifyOrderId = t2.ShopifyOrderId 
                                    AND t1.ShopifyOrderLineId = t2.ShopifyOrderLineId ";

            if (PwShop.ProfitRealization == ProfitRealization.PaymentClears)
            {
                refundQuery += "WHERE t1.PwShopId = @PwShopId AND t2.FinancialStatus IN ( 3, 4, 5, 6 ); ";
            }
            else
            {
                refundQuery += @"WHERE t1.PwShopId = @PwShopId; ";
            }
            return refundQuery;
        }

        private string RefreshInsertAdjustmentQuery()
        {
            var adjustmentQuery =
                @"INSERT INTO profitwiseprofitreportentry
                    SELECT t1.PwShopId, t1.AdjustmentDate, 3 AS EntryType, t1.ShopifyOrderId, 
                        t1.ShopifyAdjustmentId AS SourceId, NULL, NULL, t1.Amount AS NetSales, 0 AS CoGS, NULL AS Quantity
                    FROM shopifyorderadjustment t1 
                        INNER JOIN shopifyorder t2 ON t1.ShopifyOrderId = t2.ShopifyOrderId ";

            if (PwShop.ProfitRealization == ProfitRealization.PaymentClears)
            {
                adjustmentQuery += "WHERE t1.PwShopId = @PwShopId AND t2.FinancialStatus IN ( 3, 4, 5, 6 ); ";
            }
            else
            {
                adjustmentQuery += @"WHERE t1.PwShopId = @PwShopId; ";
            }
            return adjustmentQuery;
        }

        private string RefreshDeleteQuery()
        {
            var deleteQuery = @"DELETE FROM profitwiseprofitreportentry WHERE PwShopId = @PwShopId; ";
            return deleteQuery;
        }
    }
}

