using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.GoodsOnHand;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;


namespace ProfitWise.Data.Repositories
{
    public class GoodsOnHandRepository
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;
        private readonly MultitenantFactory _factory;


        public GoodsOnHandRepository(
                ConnectionWrapper connectionWrapper, MultitenantFactory factory)
        {
            _connectionWrapper = connectionWrapper;
            _factory = factory;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }

        public void PopulateQueryStub(long reportId)
        {
            var filterRepository = _factory.MakeReportFilterRepository(this.PwShop);

            var deleteQuery =
                @"DELETE FROM profitwisegoodsonhandquerystub
                WHERE PwShopId = @PwShopId AND PwReportId = @PwReportId";
            Connection.Execute(deleteQuery, new { PwShopId, PwReportId = reportId });

            var createQuery =
                @"INSERT INTO profitwisegoodsonhandquerystub
                SELECT @PwReportId, @PwShopId, PwVariantId, PwProductId, 
                        Vendor, ProductType, ProductTitle, Sku, VariantTitle
                FROM vw_standaloneproductandvariantsearch 
                WHERE PwShopId = @PwShopId " +
                filterRepository.ReportFilterClauseGenerator(reportId) +
                @" GROUP BY PwVariantId, PwProductId, 
                Vendor, ProductType, ProductTitle, Sku, VariantTitle; ";
            Connection.Execute(createQuery, new { PwShopId, PwReportId = reportId });
        }
        
        public Totals RetrieveTotals(long reportId)
        {
            var query = CTE_Query +
                @" SELECT @PwReportId AS ReportId, 
                        SUM(CostOfGoodsOnHand) AS TotalCostOfGoodsOnHand, 
                        SUM(PotentialRevenue) AS TotalPotentialRevenue
                FROM Data_CTE ;";

            return Connection.Query<Totals>(
                query, new { QueryDate = DateTime.Today, PwShopId, PwReportId = reportId }).First();
        }

        public List<Details> RetrieveDetailsByProductType(long reportId)
        {
            var query = CTE_Query +
                @" SELECT	t2.ProductType AS GroupingKey, t2.ProductType AS GroupingName, 
		                SUM(Inventory) AS TotalInventory,
		                MIN(Price) AS MinimumPrice,
		                MAX(Price) AS MaximumPrice, 
		                SUM(CostOfGoodsOnHand) AS TotalCostOfGoodsSold, 
		                SUM(PotentialRevenue) AS TotalPotentialRevenue,	
		                SUM(PotentialRevenue) - SUM(CostOfGoodsOnHand) AS TotalPotentialProfit
                FROM Data_CTE t1
	                INNER JOIN profitwiseproduct t2 ON t1.PwProductId = t2.PwProductId
                WHERE PwShopId = @PwShopId
                GROUP BY t2.ProductType 
                ORDER BY t2.ProductType ASC;";

            return Connection.Query<Details>(
                query, new { QueryDate = DateTime.Today, PwShopId, PwReportId = reportId }).ToList();
        }

        public string CTE_Query =
            @"WITH Data_CTE ( PwProductId, PwVariantId, VariantTitle, Sku, Inventory, Price, CostOfGoodsOnHand, PotentialRevenue )
                AS (
	                SELECT	t2.PwProductId, t2.PwVariantId, t2.Title, t2.Sku, t2.Inventory, t2.HighPrice AS Price, 
			                ISNULL(t2.Inventory * (t4.PercentMultiplier / 100 * t2.HighPrice + t4.FixedAmount * t5.Rate), 0) AS CostOfGoodsOnHand,
                            ISNULL(t2.Inventory, 0) * t2.HighPrice AS PotentialRevenue
	                FROM profitwisemastervariant t1
		                INNER JOIN profitwisevariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId	
		                LEFT JOIN profitwisemastervariantcogscalc t4 
			                ON t1.PwShopId = t4.PwShopId AND t1.PwMasterVariantId = t4.PwMasterVariantId 
			                AND t4.StartDate <= @QueryDate AND t4.EndDate > @QueryDate
		                LEFT JOIN exchangerate t5 ON t4.SourceCurrencyId = t5.SourceCurrencyId
				                AND t5.Date = @QueryDate AND t5.DestinationCurrencyId = 1 
	                WHERE t1.PwShopId = @PwShopId
	                AND t1.StockedDirectly = 1
	                AND t2.PwVariantId IN ( 
		                SELECT PwVariantId FROM profitwisegoodsonhandquerystub 
		                WHERE PwShopId = @PwShopId AND PwReportId = @PwReportId )
	                AND t2.PwShopId = @PwShopId
	                AND t2.Inventory IS NOT NULL
	                AND t2.IsActive = 1 
                )  ";
    }
}

