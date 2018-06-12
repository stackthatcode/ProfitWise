using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.GoodsOnHand;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Services;


namespace ProfitWise.Data.Repositories.Multitenant
{
    public class GoodsOnHandRepository
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;
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
            return _connectionWrapper.InitiateTransaction();
        }

        public void PopulateQueryStub(long reportId)
        {
            var filterRepository = _factory.MakeReportFilterRepository(this.PwShop);

            var deleteQuery =
                @"DELETE FROM goodsonhandquerystub(@PwShopId)
                WHERE PwReportId = @PwReportId";
            _connectionWrapper.Execute(deleteQuery, new { PwShopId, PwReportId = reportId });

            var createQuery =
                @"INSERT INTO goodsonhandquerystub(@PwShopId)
                SELECT @PwReportId, @PwShopId, PwVariantId, PwProductId, 
                        Vendor, ProductType, ProductTitle, Sku, VariantTitle
                FROM vw_goodsonhand 
                WHERE PwShopId = @PwShopId " +
                filterRepository.ReportFilterClauseGenerator(reportId) +
                @" GROUP BY PwVariantId, PwProductId, 
                Vendor, ProductType, ProductTitle, Sku, VariantTitle; ";
            _connectionWrapper.Execute(createQuery, new { PwShopId, PwReportId = reportId });
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

            return _connectionWrapper.Query<Totals>(
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
		            SELECT PwVariantId FROM goodsonhandquerystub(@PwShopId) 
		            WHERE PwReportId = @reportId )
                AND PwShopId = @PwShopId ";

            if (productType != null)
                query += "AND ProductType = @productType ";
            if (vendor != null)
                query += "AND Vendor = @vendor ";
            if (pwProductId != null)
                query += "AND PwProductId = @pwProductId ";

            return _connectionWrapper.Query<int>(
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
		                    CAST(MIN(Price) AS decimal(18, 2)) AS MinimumPrice,
		                    CAST(MAX(Price) AS decimal(18, 2)) AS MaximumPrice, 
		                    CAST(SUM(CostOfGoodsOnHand) AS decimal(18, 2)) AS TotalCostOfGoodsSold, 
		                    CAST(SUM(PotentialRevenue) AS decimal(18, 2)) AS TotalPotentialRevenue,	
		                    CAST(SUM(PotentialRevenue) - SUM(CostOfGoodsOnHand) AS decimal(18, 2)) AS TotalPotentialProfit
                        FROM Data_CTE t1
	                        INNER JOIN product(@PwShopId) t2 ON t1.PwProductId = t2.PwProductId
                        WHERE PwShopId = @PwShopId ";

            if (productType != null)
                query += "AND t2.ProductType = @productType ";
            if (vendor != null)
                query += "AND t2.Vendor = @vendor ";
            if (pwProductId != null)
                query += "AND t2.PwProductId = @pwProductId ";

            query += GroupByClause(grouping) + " " +
                    OrderByClauseAggregate(ordering, grouping) + " " +
                    "OFFSET @StartingIndex ROWS FETCH NEXT @pageSize ROWS ONLY;";

            var today = _timeZoneTranslator.Today(PwShop.TimeZone);

            return _connectionWrapper.Query<Details>(query, new {
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
        
        private string OrderByClauseAggregate(
                ColumnOrdering ordering, ReportGrouping reportGrouping)
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

            if (ordering == ColumnOrdering.NameAscending || ordering == ColumnOrdering.NameDescending)
            {
                var direction = ordering == ColumnOrdering.NameAscending ? " ASC " : " DESC ";

                if (reportGrouping == ReportGrouping.ProductType)
                {
                    return $" ORDER BY t2.ProductType " + direction;
                }
                if (reportGrouping == ReportGrouping.Vendor)
                {
                    return $" ORDER BY t2.Vendor " + direction;
                }
                if (reportGrouping == ReportGrouping.Product)
                {
                    return $" ORDER BY t2.Title " + direction;
                }
                if (reportGrouping == ReportGrouping.Variant)
                {
                    return $@" ORDER BY t1.SKU + ' - ' + t2.Title + ' - ' + t1.VariantTitle " + direction;
                }
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
            @"WITH Data_CTE ( 
                PwProductId, PwVariantId, VariantTitle, Sku, Inventory, Price, CostOfGoodsOnHand, PotentialRevenue 
            ) AS (
                SELECT  PwProductId, PwVariantId, Title, Sku, Inventory, CurrentUnitPrice, 
                        Inventory * UnitCogsByDate AS CostOfGoodsOnHand, Inventory * CurrentUnitPrice AS PotentialRevenue
                FROM dbo.costofgoodsbydate(@PwShopId, @QueryDate)
                WHERE PwVariantId IN ( 
                    SELECT PwVariantId FROM goodsonhandquerystub(@PwShopId) WHERE PwReportId = @PwReportId 
                )
                AND StockedDirectly = 1
                AND Inventory IS NOT NULL
                AND IsActive = 1
            ) ";
    }
}

