using System;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Preferences;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwCogsDataUpdateRepository : IShopFilter
    {
        private readonly MySqlConnection _connection;
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;


        public PwCogsDataUpdateRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        // Order Line update queries
        public void UpdateOrderLines(OrderLineCogsContext lineContext)
        {
            if (lineContext.PwMasterVariantId == null && lineContext.PwMasterProductId == null)
            {
                throw new ArgumentNullException("PwMasterVariantId and PwMasterProductId can't both be null");
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

        public void UpdateOrderLineFixedAmount(OrderLineCogsContext lineContext)
        {
            var query =
                @"UPDATE profitwisemastervariant t1 
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	           
                    LEFT JOIN exchangerate t4
		                ON Date(t3.OrderDate) = t4.`Date` 
			                AND t4.SourceCurrencyId = @CogsCurrencyId
			                AND t4.DestinationCurrencyId = @DestinationCurrencyId
                SET t3.UnitCogs = (@CogsAmount * IFNULL(t4.Rate, 0)) 
                WHERE t1.PwShopId = @PwShopId " +
                WhereClauseGenerator(lineContext);

            _connection.Execute(query, lineContext);
        }

        public void UpdateOrderLinePercentage(OrderLineCogsContext lineContext)
        {
            var query =
                @"UPDATE profitwisemastervariant t1 
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	               
                
                SET t3.UnitCogs = @CogsPercentOfUnitPrice * t3.UnitPrice / 100.00
                WHERE t1.PwShopId = @PwShopId " + 
                WhereClauseGenerator(lineContext);

            _connection.Execute(query, lineContext);
        }

        public void UpdateOrderLineFixedAmount(OrderLineCogsContextPickList context)
        {
            var query =
                @"UPDATE profitwisepicklistmasterproduct t0
                    INNER JOIN profitwisemastervariant t1 
                        ON t0.PwShopId = t1.PwShopId AND t0.PwMasterProductId = t1.PwMasterProductId
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	           
                    LEFT JOIN exchangerate t4
		                ON Date(t3.OrderDate) = t4.`Date` 
			                AND t4.SourceCurrencyId = @CogsCurrencyId
			                AND t4.DestinationCurrencyId = @DestinationCurrencyId
                SET t3.UnitCogs = (@CogsAmount * IFNULL(t4.Rate, 0)) 
                WHERE t1.PwShopId = @PwShopId AND t0.PwPickListId = @PwPickListId";
                
            _connection.Execute(query, context);
        }

        public void UpdateOrderLinePercentage(OrderLineCogsContextPickList context)
        {
            var query =
                @"UPDATE profitwisepicklistmasterproduct t0
                    INNER JOIN profitwisemastervariant t1 
                        ON t0.PwShopId = t1.PwShopId AND t0.PwMasterProductId = t1.PwMasterProductId
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
                SET t3.UnitCogs = @CogsPercentOfUnitPrice * t3.UnitPrice / 100.00
                WHERE t1.PwShopId = @PwShopId AND t0.PwPickListId = @PwPickListId";

            _connection.Execute(query, context);
        }

        public string WhereClauseGenerator(OrderLineCogsContext lineContext)
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
        

        // Report Entry query
        public void RefreshReportEntryData()
        {
            var query =
                RefreshDeleteQuery() +
                RefreshInsertLineItemQuery() +
                RefreshInsertRefundQuery() +
                RefreshInsertAdjustmentQuery();

            _connection.Execute(query, new { PwShopId, DefaultCogsPercent = PwShop.DefaultCogsPercent });
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
                adjustmentQuery += "WHERE t1.PwShopId = @PwShopId AND t2.FinancialStatus IN ( 4, 5, 6 ); ";
            }
            else
            {
                adjustmentQuery += @"WHERE t1.PwShopId = @PwShopId; ";
            }
            return adjustmentQuery;
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
                    "IF (UnitCogs = 0 OR UnitCogs IS NULL, -t1.RestockQuantity * UnitPrice * @DefaultCogsPercent , -t1.RestockQuantity * UnitCogs) AS CoGS, ";
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
                refundQuery += "WHERE t1.PwShopId = @PwShopId AND t2.FinancialStatus IN ( 4, 5, 6 ); ";
            }
            else
            {
                refundQuery += @"WHERE t1.PwShopId = @PwShopId; ";
            }
            return refundQuery;
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
                    "IF (UnitCogs = 0 OR UnitCogs IS NULL,  Quantity * UnitPrice * @DefaultCogsPercent , Quantity * UnitCogs) AS CoGS, ";
            }
            else
            {
                orderQuery += "(Quantity * UnitCogs) AS CoGS, ";
            }
            orderQuery += @"Quantity AS Quantity FROM shopifyorderlineitem ";
            if (PwShop.ProfitRealization == ProfitRealization.PaymentClears)
            {
                orderQuery += "WHERE PwShopId = @PwShopId AND FinancialStatus IN ( 4, 5, 6 ); ";
            }
            else
            {
                orderQuery += @"WHERE PwShopId = @PwShopId; ";
            }
            return orderQuery;
        }

        private string RefreshDeleteQuery()
        {
            var deleteQuery = @"DELETE FROM profitwiseprofitreportentry WHERE PwShopId = @PwShopId; ";
            return deleteQuery;
        }
    }
}

