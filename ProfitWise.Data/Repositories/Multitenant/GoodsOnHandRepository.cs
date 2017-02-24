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
using ProfitWise.Data.Services;


namespace ProfitWise.Data.Repositories
{
    public class GoodsOnHandRepository
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;
        private readonly MultitenantFactory _factory;
        private readonly TimeZoneTranslator _timeZoneTranslator;

        public GoodsOnHandRepository(
                ConnectionWrapper connectionWrapper, 
                MultitenantFactory factory,
                TimeZoneTranslator timeZoneTranslator)
        {
            _connectionWrapper = connectionWrapper;
            _factory = factory;
            _timeZoneTranslator = timeZoneTranslator;
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
                FROM vw_goodsonhand 
                WHERE PwShopId = @PwShopId " +
                filterRepository.ReportFilterClauseGenerator(reportId) +
                @" GROUP BY PwVariantId, PwProductId, 
                Vendor, ProductType, ProductTitle, Sku, VariantTitle; ";
            Connection.Execute(createQuery, new { PwShopId, PwReportId = reportId });
        }


        private decimal MarginMultiplier()
        {
            return PwShop.UseDefaultMargin ? PwShop.DefaultMargin / 100m : 0m;
        }

        public Totals RetrieveTotals(long reportId)
        {
            var query = CTE_Query +
                @" SELECT @PwReportId AS ReportId, 
                        SUM(CostOfGoodsOnHand) AS TotalCostOfGoodsOnHand, 
                        SUM(PotentialRevenue) AS TotalPotentialRevenue
                FROM Data_CTE ";

            var today = _timeZoneTranslator.Today(PwShop.TimeZone);

            return Connection.Query<Totals>(
                query, new
                {
                    QueryDate = today,
                    PwShopId,
                    PwReportId = reportId,
                    MarginMultiplier = MarginMultiplier()
                }).First();
        }

        public int DetailsCount(long reportId, ReportGrouping grouping,
                string productType = null, string vendor = null, long? pwProductId = null)
        {
            var selectQueryHead = "";
            if (grouping == ReportGrouping.Product)
                selectQueryHead = " SELECT COUNT(DISTINCT(PwProductId)) ";
            if (grouping == ReportGrouping.Variant)
                selectQueryHead = " SELECT COUNT(DISTINCT(PwVariantId)) ";
            if (grouping == ReportGrouping.ProductType)
                selectQueryHead = " SELECT COUNT(DISTINCT(ProductType)) ";
            if (grouping == ReportGrouping.Vendor)
                selectQueryHead = " SELECT COUNT(DISTINCT(Vendor)) ";

            var query =
                selectQueryHead +
                @" FROM vw_goodsonhand
                WHERE PwVariantId IN ( 
		            SELECT PwVariantId FROM profitwisegoodsonhandquerystub 
		            WHERE PwShopId = @PwShopId AND PwReportId = @reportId )
                AND PwShopId = @PwShopId ";

            if (productType != null)
                query += "AND ProductType = @productType ";
            if (vendor != null)
                query += "AND Vendor = @vendor ";
            if (pwProductId != null)
                query += "AND PwProductId = @pwProductId ";

            return Connection.Query<int>(
                query, new { PwShopId, reportId, productType, vendor, pwProductId }).FirstOrDefault();
        }

        public List<Details> RetrieveDetails(
                long reportId, ReportGrouping grouping, ColumnOrdering ordering, 
                int pageNumber, int pageSize, 
                string productType = null, string vendor = null, long? pwProductId = null)
        {
            var query = CTE_Query + " " +
                        SelectGroupingKeyAndName(grouping) +
                        @"  SUM(Inventory) AS TotalInventory,
		                    MIN(Price) AS MinimumPrice,
		                    MAX(Price) AS MaximumPrice, 
		                    SUM(CostOfGoodsOnHand) AS TotalCostOfGoodsSold, 
		                    SUM(PotentialRevenue) AS TotalPotentialRevenue,	
		                    SUM(PotentialRevenue) - SUM(CostOfGoodsOnHand) AS TotalPotentialProfit
                        FROM Data_CTE t1
	                        INNER JOIN profitwiseproduct t2 ON t1.PwProductId = t2.PwProductId
                        WHERE PwShopId = @PwShopId ";

            if (productType != null)
                query += "AND t2.ProductType = @productType ";
            if (vendor != null)
                query += "AND t2.Vendor = @vendor ";
            if (pwProductId != null)
                query += "AND t2.PwProductId = @pwProductId ";

            query += GroupByClause(grouping) + " " +
                    OrderByClauseAggregate(ordering) + " " +
                    "OFFSET @StartingIndex ROWS FETCH NEXT @pageSize ROWS ONLY;";

            var today = _timeZoneTranslator.Today(PwShop.TimeZone);

            return Connection.Query<Details>(query, new {
                    QueryDate = today,
                    PwShopId,
                    PwReportId = reportId,
                    MarginMultiplier = MarginMultiplier(),
                    pageSize,
                    startingIndex = (pageNumber - 1) * pageSize,
                    productType,
                    vendor,
                    pwProductId,
                }).ToList();
        }
        
        private string OrderByClauseAggregate(ColumnOrdering ordering)
        {
            if (ordering == ColumnOrdering.InventoryAscending)
            {
                return "ORDER BY SUM(Inventory) ASC";
            }
            if (ordering == ColumnOrdering.InventoryDescending)
            {
                return "ORDER BY SUM(Inventory) DESC";
            }

            if (ordering == ColumnOrdering.CogsAscending)
            {
                return "ORDER BY SUM(CostOfGoodsOnHand) ASC";
            }
            if (ordering == ColumnOrdering.CogsDescending)
            {
                return "ORDER BY SUM(CostOfGoodsOnHand) DESC";
            }

            if (ordering == ColumnOrdering.PotentialRevenueAscending)
            {
                return "ORDER BY SUM(PotentialRevenue) ASC";
            }
            if (ordering == ColumnOrdering.PotentialRevenueDescending)
            {
                return "ORDER BY SUM(PotentialRevenue) DESC";
            }

            if (ordering == ColumnOrdering.PotentialProfitAscending)
            {
                return "ORDER BY SUM(PotentialRevenue) - SUM(CostOfGoodsOnHand) ASC";
            }
            if (ordering == ColumnOrdering.PotentialProfitDescending)
            {
                return "ORDER BY SUM(PotentialRevenue) - SUM(CostOfGoodsOnHand) DESC";
            }
            throw new ArgumentException("reportGrouping");
        }        

        private string GroupByClause(ReportGrouping reportGrouping)
        {
            if (reportGrouping == ReportGrouping.ProductType)
            {
                return $"GROUP BY t2.ProductType ";
            }
            if (reportGrouping == ReportGrouping.Vendor)
            {
                return $"GROUP BY t2.Vendor ";
            }
            if (reportGrouping == ReportGrouping.Product)
            {
                return $"GROUP BY t2.PwProductId, t2.Title ";
            }
            if (reportGrouping == ReportGrouping.Variant)
            {
                return $@"GROUP BY t1.PwVariantId, t1.SKU + ' - ' + t2.Title + ' - ' + t1.VariantTitle";
            }
            throw new ArgumentException("reportGrouping");
        }

        private string SelectGroupingKeyAndName(ReportGrouping reportGrouping)
        {
            if (reportGrouping == ReportGrouping.ProductType)
            {
                return $"SELECT t2.ProductType AS GroupingKey, t2.ProductType AS GroupingName, ";
            }
            if (reportGrouping == ReportGrouping.Vendor)
            {
                return $"SELECT t2.Vendor AS GroupingKey, t2.Vendor AS GroupingName, ";
            }
            if (reportGrouping == ReportGrouping.Product)
            {
                return $"SELECT t2.PwProductId AS GroupingKey, t2.Title AS GroupingName, ";
            }
            if (reportGrouping == ReportGrouping.Variant)
            {
                return $@"SELECT t1.PwVariantId AS GroupingKey, t1.SKU + ' - ' + t2.Title + ' - ' + t1.VariantTitle AS GroupingName, ";
            }
            throw new ArgumentException("reportGrouping");
        }

        private string CTE_Query=
            @"WITH Data_CTE ( PwProductId, PwVariantId, VariantTitle, Sku, Inventory, Price, CostOfGoodsOnHand, PotentialRevenue )
                AS (
                    SELECT	t2.PwProductId, t2.PwVariantId, t2.Title, t2.Sku, dbo.ufnNegToZero(t2.Inventory), t2.HighPrice AS Price, 
                            CASE WHEN (t4.PercentMultiplier = 0 AND t4.FixedAmount = 0 AND @MarginMultiplier <> 0) THEN @MarginMultiplier * t2.HighPrice * dbo.ufnNegToZero(t2.Inventory)
                            ELSE (ISNULL(t4.PercentMultiplier, 0) / 100 * t2.HighPrice + ISNULL(t4.FixedAmount, 0) * t5.Rate) * dbo.ufnNegToZero(t2.Inventory) END
                            AS CostOfGoodsOnHand,
                            dbo.ufnNegToZero(t2.Inventory) * t2.HighPrice AS PotentialRevenue
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

