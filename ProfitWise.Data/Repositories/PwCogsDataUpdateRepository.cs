using System;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
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

        // Order Line CoGS propagation functions...
        public void UpdateUnitCogsByFixedAmount(
                long? pwMasterProductId = null, long? pwMasterVariantId = null)
        {
            if (pwMasterProductId == null && pwMasterVariantId == null)
            {
                throw new ArgumentNullException("Both pwMasterProductId and pwMasterVariantId can't be null");
            }

            var query =
                @"UPDATE profitwiseshop t0
                    INNER JOIN profitwisemastervariant t1 
		                ON t0.PwShopId = t1.PwShopId
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId
	                LEFT JOIN exchangerate t4
		                ON Date(t3.OrderDate) = t4.`Date` 
			                AND t4.SourceCurrencyId = t1.CogsCurrencyId
			                AND t4.DestinationCurrencyId = t0.CurrencyId
                SET t3.UnitCogs = (t1.CogsAmount * IFNULL(t4.Rate, 0)) ";

            var whereClause = " WHERE t0.PwShopId = @PwShopId ";
            if (pwMasterProductId.HasValue)
            {
                whereClause += "AND t1.PwMasterProductId = @pwMasterProductId ";
            }
            if (pwMasterVariantId.HasValue)
            {
                whereClause += "AND t1.PwMasterVariantId = @pwMasterVariantId ";
            }

            _connection.Execute(
                query, new { PwShopId, pwMasterProductId, pwMasterVariantId });
        }

        public void UpdateUnitCogsByPercentage(
        long? pwMasterProductId = null, long? pwMasterVariantId = null)
        {
            if (pwMasterProductId == null && pwMasterVariantId == null)
            {
                throw new ArgumentNullException("Both pwMasterProductId and pwMasterVariantId can't be null");
            }

            var query =
                @"UPDATE profitwiseshop t0
                    INNER JOIN profitwisemastervariant t1 
		                ON t0.PwShopId = t1.PwShopId
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN shopifyorderlineitem t3
		                ON t2.PwShopID = t3.PwShopId AND t2.PwProductId = t3.PwProductId AND t2.PwVariantId = t3.PwVariantId	                
                SET t3.UnitCogs = (t1.CogsAmount * IFNULL(t4.Rate, 0)) ";

            var whereClause = " WHERE t0.PwShopId = @PwShopId ";
            if (pwMasterProductId.HasValue)
            {
                whereClause += "AND t1.PwMasterProductId = @pwMasterProductId ";
            }
            if (pwMasterVariantId.HasValue)
            {
                whereClause += "AND t1.PwMasterVariantId = @pwMasterVariantId ";
            }

            _connection.Execute(
                query, new { PwShopId, pwMasterProductId, pwMasterVariantId });
        }

        // Report Entry query
        public void RefreshReportEntryData()
        {
            var query =
                @"DELETE FROM profitwiseprofitreportentry WHERE PwShopId = @PwShopId;
                
                INSERT INTO profitwiseprofitreportentry
                SELECT 	PwShopId, OrderDate, 1 AS EntryType, ShopifyOrderId, ShopifyOrderLineId AS SourceId, 
		                PwProductId, PwVariantId, TotalAfterAllDiscounts AS NetSales, Quantity * UnitCogs AS CoGS,
                        Quantity AS Quantity
                FROM shopifyorderlineitem
                WHERE PwShopId = @PwShopId;

                INSERT INTO profitwiseprofitreportentry
                SELECT 	t1.PwShopId, t1.RefundDate, 2 AS EntryType, t1.ShopifyOrderId, t1.ShopifyRefundId AS SourceId, 
		                t1.PwProductId, t1.PwVariantId, -t1.Amount AS NetSales, -t1.RestockQuantity * t2.UnitCogs AS CoGS,
                        -t1.RestockQuantity AS Quantity
                FROM shopifyorderrefund t1
		                INNER JOIN shopifyorderlineitem t2
			                ON t1.PwShopId = t2.PwShopId 
                            AND t1.ShopifyOrderId = t2.ShopifyOrderId 
                            AND t1.ShopifyOrderLineId = t2.ShopifyOrderLineId
                WHERE t1.PwShopId = @PwShopId;

                INSERT INTO profitwiseprofitreportentry
                SELECT t1.PwShopId, t1.AdjustmentDate, 3 AS EntryType, t1.ShopifyOrderId, t1.ShopifyAdjustmentId AS SourceId, 
		                NULL, NULL, t1.Amount AS NetSales, 0 AS CoGS, NULL AS Quantity
                FROM shopifyorderadjustment t1
                WHERE t1.PwShopId = @PwShopId;";

            _connection.Execute(query, new { PwShopId });
        }

    }
}
