using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Services;

namespace ProfitWise.Data.Repositories.Multitenant
{
    [Intercept(typeof(ShopRequired))]
    public class CogsEntryRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly TimeZoneTranslator _timeZoneTranslator;

        public CogsEntryRepository(
                ConnectionWrapper connectionWrapper, 
                TimeZoneTranslator timeZoneTranslator)
        {
            _connectionWrapper = connectionWrapper;
            _timeZoneTranslator = timeZoneTranslator;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }

        public PwCogsProductSummary RetrieveProduct(long masterProductId)
        {
            var query =
                @"SELECT PwMasterProductId, PwProductId, Title, Vendor
                FROM product(@PwShopId)
                WHERE PwMasterProductId = @PwMasterProductId AND IsPrimary = 1;";

            var output =
                _connectionWrapper
                    .Query<PwCogsProductSummary>(query, new { PwShop.PwShopId, PwMasterProductId = masterProductId })
                    .FirstOrDefault();

            output.DateToday = _timeZoneTranslator.Today(this.PwShop.TimeZone);
            return output;
        }
        
        public void TouchPickList(long pickListId)
        {
            var touchQuery =
                @"UPDATE picklist(@PwShopId) SET LastAccessed = @now WHERE PwPickListId = @pickListId";
            _connectionWrapper.Execute(touchQuery, new { PwShopId, pickListId, now = DateTime.UtcNow });
        }

        private string ResultsQueryGen(int sortByColumn, bool sortByDirectionDown, bool primaryOnly = true)
        {
            var sortDirectionWord = (sortByDirectionDown ? "ASC" : "DESC");
            var sortByClause =
                "ORDER BY " +
                (sortByColumn == 0
                    ? $"Vendor {sortDirectionWord}, Title {sortDirectionWord} "
                    : $"Title {sortDirectionWord}, Vendor {sortDirectionWord} ");

            var isPrimaryClause = primaryOnly ? "AND IsPrimary = 1 " : "";

            var query =
                @"SELECT PwMasterProductId, PwProductId, Title, Vendor
                FROM product(@PwShopId) WHERE PwShopId = PwShopId " + 
                isPrimaryClause +
                @"AND PwMasterProductId IN ( 
	                SELECT PwMasterProductId FROM picklistmasterproduct(@PwShopId) WHERE PwPickListId = @pickListId ) " +
                sortByClause + " OFFSET @StartRecord ROWS FETCH NEXT @ResultsPerPage ROWS ONLY;";            
            
            return query;
        }

        // Sort By Column { 0 for Vendor, 1 for Product Title }
        public IList<PwCogsProductSummary> RetrieveCogsSummaryFromPicklist(
                long pickListId, int pageNumber, int resultsPerPage, int sortByColumn = 0, bool sortByDirectionDown = true)
        {
            if (resultsPerPage > 200)
            {
                throw new ArgumentException("Maximum number of results per page is 200");
            }

            TouchPickList(pickListId);

            var query = ResultsQueryGen(sortByColumn, sortByDirectionDown);
            var startRecord = (pageNumber - 1) * resultsPerPage;

            var output = _connectionWrapper.Query<PwCogsProductSummary>(
                query, new { PwShop.PwShopId, pickListId, StartRecord = startRecord, ResultsPerPage = resultsPerPage }).ToList();

            var today = _timeZoneTranslator.Today(this.PwShop.TimeZone);
            output.ForEach(x => x.DateToday = today);
            return output;
        }        

        public IList<PwCogsVariantSummary> RetrieveVariants(IList<long> masterProductIds)
        {
            var query =
                $@"WITH CTE_PriceSummary ( PwMasterVariantId, LowPriceAll, HighPriceAll, TotalInventory ) AS 
                (
	                SELECT t2.PwMasterVariantId, MAX(t3.HighPrice), MIN(t3.LowPrice), SUM(Inventory)
	                FROM mastervariant(@PwShopId) t2 
		                INNER JOIN variant(@PwShopId) t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
	                WHERE t2.PwMasterProductId IN @MasterProductIds
	                GROUP BY t2.PwMasterVariantId
                )
                SELECT t2.PwMasterProductId, t2.PwMasterVariantId, t3.Title, t4.Title AS ProductTitle, t3.Sku, 
                    t2.Exclude, t2.StockedDirectly, t2.CogsTypeId, t2.CogsMarginPercent, t2.CogsCurrencyId, 
                    t2.CogsAmount, t2.CogsDetail, t5.LowPriceAll AS LowPrice, t5.HighPriceAll AS HighPrice, t5.TotalInventory AS Inventory
                FROM mastervariant(@PwShopId) t2 
	                INNER JOIN variant(@PwShopId) t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
                    INNER JOIN product(@PwShopId) t4 ON t3.PwProductId = t4.PwProductId
	                INNER JOIN CTE_PriceSummary t5 ON t3.PwMasterVariantId = t5.PwMasterVariantId
                WHERE t3.IsPrimary = 1 AND t2.PwMasterProductId IN @MasterProductIds;";

            var output = _connectionWrapper.Query<PwCogsVariantSummary>(
                query, new {this.PwShopId, MasterProductIds = masterProductIds}).ToList();
            var today = _timeZoneTranslator.Today(this.PwShop.TimeZone);
            output.ForEach(x => x.DateToday = today);
            return output;
        }

        public PwCogsVariantSummary RetrieveVariant(long masterVariantId)
        {
            var query =
                @"SELECT t2.PwMasterProductId, t2.PwMasterVariantId, t3.Title, t3.Sku, t2.Exclude, t2.StockedDirectly, 
                        t2.CogsTypeId, t2.CogsMarginPercent, t2.CogsCurrencyId, t2.CogsAmount, t2.CogsDetail, 
                        t3.PwVariantId, t3.LowPrice, t3.HighPrice, t3.Inventory
                FROM mastervariant(@PwShopId) t2 
                        INNER JOIN variant(@PwShopId) t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
	            WHERE t3.IsPrimary = 1 AND t2.PwMasterVariantId = @masterVariantId";

            var variant =
                _connectionWrapper
                    .Query<PwCogsVariantSummary>(query, new { this.PwShopId, masterVariantId })
                    .FirstOrDefault();

            variant.DateToday = _timeZoneTranslator.Today(PwShop.TimeZone);
            return variant;
        }        


        // Stocked Directly and Exclude data
        public void UpdateStockedDirectlyByPicklist(long pickListId, bool stockedDirectly)
        {
            var query =
                @"UPDATE mastervariant(@PwShopId) 
                SET StockedDirectly = @stockedDirectly
                WHERE PwMasterProductId IN
                    (SELECT PwMasterProductId FROM picklistmasterproduct(@PwShopId) WHERE PwPickListId = @pickListId);";

            _connectionWrapper.Execute(
                query, new {PwShop.PwShopId, pickListId, stockedDirectly});
        }

        public void UpdateStockedDirectlyByMasterProductId(long masterProductId, bool stockedDirectly)
        {
            var query =
                @"UPDATE mastervariant(@PwShopId) SET StockedDirectly = @stockedDirectly
                WHERE PwMasterProductId = @masterProductId;";

            _connectionWrapper.Execute(query, new { PwShopId, masterProductId, stockedDirectly} );
        }

        public void UpdateStockedDirectlyByMasterVariantId(long masterVariantId, bool stockedDirectly)
        {
            var query =
                @"UPDATE mastervariant(@PwShopId) 
                SET StockedDirectly = @stockedDirectly
                WHERE PwMasterVariantId = @masterVariantId;";

            _connectionWrapper.Execute(query, new { PwShopId, masterVariantId, stockedDirectly});
        }

        public void UpdateExcludeByPicklist(long pickListId, bool exclude)
        {
            var query =
                @"UPDATE mastervariant(@PwShopId) SET Exclude = @exclude
                WHERE PwMasterProductId IN
                    (SELECT PwMasterProductId FROM picklistmasterproduct(@PwShopId) 
                    WHERE PwPickListId = @pickListId);";

            _connectionWrapper.Execute(query, new {PwShopId, pickListId, exclude});
        }

        public void UpdateExcludeByMasterProductId(long masterProductId, bool exclude)
        {
            var query =
                @"UPDATE mastervariant(@PwShopId) 
                SET Exclude = @exclude
                WHERE PwMasterProductId = @masterProductId;";

            _connectionWrapper.Execute(query, new {PwShopId, masterProductId, exclude});
        }

        public void UpdateExcludeByMasterVariantId(long masterVariantId, bool exclude)
        {
            var query =
                @"UPDATE mastervariant(@PwShopId) 
                SET Exclude = @exclude
                WHERE PwMasterVariantId = @masterVariantId;";

            _connectionWrapper.Execute(query, new {PwShopId, masterVariantId, exclude});
        }


        // Product Search input
        public IList<string> RetrieveVendors()
        {
            var query = @"SELECT DISTINCT Vendor AS Vendor 
                        FROM product(@PwShopId) 
                        WHERE Vendor IS NOT NULL AND Vendor <> ''
                        ORDER BY Vendor;";

            return _connectionWrapper.Query<string>(
                query, new {PwShopId = this.PwShop.PwShopId}).ToList();
        }

        public IList<string> RetrieveProductType()
        {
            var query = @"SELECT DISTINCT ProductType AS ProductType
                        FROM product(@PwShopId) 
                        WHERE ProductType IS NOT NULL AND ProductType <> ''
                        ORDER BY ProductType;";

            return _connectionWrapper.Query<string>(query, new { PwShop.PwShopId}).ToList();
        }


        // Master Variant Cogs Defaults
        public void UpdateMasterVariantDefaultCogs(
                long masterVariantId, PwCogsDetail input, bool hasDetail)
        {
            UpdateMasterVariantDefaultCogs(
                masterVariantId, input.CogsTypeId, input.CogsCurrencyId, input.CogsAmount, 
                input.CogsMarginPercent, hasDetail);
        } 
       
        public void UpdateMasterVariantDefaultCogs(
                    long masterVariantId, int cogsTypeId, int? cogsCurrencyId, decimal? cogsAmount, 
                    decimal? cogsMarginPercent, bool cogsDetail)
        {
            if (cogsTypeId != CogsType.FixedAmount && cogsTypeId != CogsType.MarginPercentage)
            {
                throw new ArgumentException("Invalid cogstypeId");
            }

            var query =
                @"UPDATE mastervariant(@PwShopId)
                SET CogsCurrencyId = @cogsCurrencyId, 
                    CogsAmount = @cogsAmount,
                    CogsTypeId = @cogsTypeId,
                    CogsMarginPercent = @cogsMarginPercent,
                    CogsDetail = @cogsDetail                   
                WHERE PwMasterVariantId = @masterVariantId;";

            _connectionWrapper.Execute(query, 
                new { PwShopId, masterVariantId,cogsTypeId, cogsCurrencyId, cogsAmount, cogsMarginPercent, cogsDetail });
        }

        public void UpdatePickListDefaultCogs(long pickListId, PwCogsDetail input, bool cogsDetail)
        {
            var query =
                @"UPDATE t1        
                SET CogsCurrencyId = @CogsCurrencyId, 
                    CogsAmount = @CogsAmount,
                    CogsTypeId = @CogsTypeId,
                    CogsMarginPercent = @CogsMarginPercent,
                    CogsDetail = @cogsDetail   
                FROM mastervariant(@PwShopId) t1
                    INNER JOIN picklistmasterproduct(@PwShopId) t2
                        ON t1.PwMasterProductId = t2.PwMasterProductId                               
                WHERE PwPickListId = @pickListId;";

            _connectionWrapper.Execute(
                query, new
                {
                    this.PwShopId,
                    pickListId,
                    input.CogsTypeId,
                    input.CogsCurrencyId,
                    input.CogsAmount,
                    input.CogsMarginPercent,
                    cogsDetail,
                });
        }


        // CoGS Detail functions
        public List<PwCogsDetail> RetrieveCogsDetailByMasterVariant(long masterVariantId)
        {
            var query = 
                @"SELECT * FROM mastervariantcogsdetail(@PwShopId) 
                WHERE PwMasterVariantId = @masterVariantId ORDER BY CogsDate;";

            return _connectionWrapper.Query<PwCogsDetail>(
                query, new { this.PwShop.PwShopId, @masterVariantId }).ToList();
        }

        public List<PwCogsDetail> RetrieveCogsDetailByMasterProduct(long masterProductId)
        {
            var query =
                @"SELECT t1.* 
                    FROM mastervariantcogsdetail(@PwShopId) t1
                        INNER JOIN mastervariant(@PwShopId) t2 
                            ON t1.PwMasterVariantId = t2.PwMasterVariantId
                WHERE t2.PwMasterProductId = @masterProductId
                ORDER BY t1.CogsDate;";

            return _connectionWrapper
                    .Query<PwCogsDetail>(query, new { this.PwShop.PwShopId, masterProductId }).ToList();
        }

        public List<PwCogsDetail> RetrieveCogsDetailByMasterProduct(List<long> masterProductIds)
        {
            var query =
                @"SELECT t1.* 
                FROM mastervariantcogsdetail(@PwShopId) t1
                    INNER JOIN mastervariant(@PwShopId) t2 
                        ON t1.PwMasterVariantId = t2.PwMasterVariantId
                WHERE t2.PwMasterProductId IN @masterProductIds
                ORDER BY t1.CogsDate;";

            return _connectionWrapper
                    .Query<PwCogsDetail>(query, new { this.PwShop.PwShopId, masterProductIds }).ToList();
        }

        public List<PwCogsDetail> RetrieveCogsDetailAll()
        {
            var query = @"SELECT * FROM mastervariantcogsdetail(@PwShopId) ORDER BY CogsDate;";

            return _connectionWrapper.Query<PwCogsDetail>(query, new {this.PwShop.PwShopId}).ToList();
        }

        public void DeleteCogsDetail(long masterVariantId)
        {
            var query = 
                @"DELETE FROM mastervariantcogsdetail(@PwShopId) 
                WHERE PwMasterVariantId = @masterVariantId;";
            _connectionWrapper.Execute(query, new {this.PwShopId, masterVariantId});
        }

        public void DeleteCogsDetailByPickList(long pickListId)
        {
            var query =
                @"DELETE FROM mastervariantcogsdetail(@PwShopId) 
                WHERE PwMasterVariantId IN (
                    SELECT PwMasterVariantId
                    FROM picklistmasterproduct(@PwShopId) t1  
	                    INNER JOIN mastervariant(@PwShopId) t2 
                            ON t1.PwMasterProductId = t2.PwMasterProductId
                    WHERE t1.PwPickListId = @pickListId )";
            _connectionWrapper.Execute(query, new { this.PwShopId, pickListId });
        }

        public void InsertCogsDetails(PwCogsDetail detail)
        {
            detail.ValidateType();
            detail.PwShopId = this.PwShopId;
            var query = 
                @"INSERT INTO mastervariantcogsdetail(@PwShopId) VALUES ( 
                    @PwMasterVariantId, @PwShopId, @CogsDate, @CogsTypeId, 
                    @CogsCurrencyId, @CogsAmount, @CogsMarginPercent );";
            _connectionWrapper.Execute(query, detail);
        }

        public void InsertCogsDetailByPickList(long pickListId, PwCogsDetail detail)
        {
            detail.ValidateType();
            var query =
                @"INSERT INTO mastervariantcogsdetail(@PwShopId) 
                SELECT t2.PwMasterVariantId, @PwShopId, @CogsDate, @CogsTypeId, @CogsCurrencyId, 
                        @CogsAmount, @CogsMarginPercent
                FROM picklistmasterproduct(@PwShopId) t1  
	                    INNER JOIN mastervariant(@PwShopId) t2 
                            ON t1.PwMasterProductId = t2.PwMasterProductId
                WHERE t1.PwPickListId = @pickListId;";

            var data = new
            {
                this.PwShopId, pickListId, detail.CogsDate, detail.CogsTypeId, detail.CogsCurrencyId,
                detail.CogsAmount, detail.CogsMarginPercent
            };

            _connectionWrapper.Execute(query, data);
        }



        // Goods on Hand queries
        public void InsertCogsCalcByMasterVariant(CogsDateBlockContext context)
        {
            var query =
                @"INSERT INTO mastervariantcogscalc(@PwShopId) 
                VALUES (
                    @PwMasterVariantId, @PwShopId, @StartDate, @EndDate, 
                    @PercentMultiplier, @SourceCurrencyId, @FixedAmount  )";

            var calcContext = context.ToCalcContext();

            _connectionWrapper.Execute(
                query, new
                {
                    PwShop.PwShopId,
                    context.PwMasterVariantId,
                    context.StartDate,
                    context.EndDate,
                    calcContext.PercentMultiplier,
                    calcContext.SourceCurrencyId,
                    calcContext.FixedAmount,
                });
        }

        public void InsertCogsCalcByMasterProduct(CogsDateBlockContext context)
        {
            var query =
                @"INSERT INTO mastervariantcogscalc(@PwShopId) 
                SELECT PwMasterVariantId, @PwShopId, @StartDate, @EndDate, 
                        @PercentMultiplier, @SourceCurrencyId, @FixedAmount
                FROM mastervariant(@PwShopId) 
                WHERE PwMasterProductId = @PwMasterProductId";

            var calcContext = context.ToCalcContext();

            _connectionWrapper.Execute(
                query, new {
                    PwShop.PwShopId, context.PwMasterProductId, context.StartDate, context.EndDate,
                    calcContext.PercentMultiplier, calcContext.SourceCurrencyId, calcContext.FixedAmount,
                });
        }

        public void InsertCogsCalcByPickList(CogsDateBlockContext context)
        {
            var query =
                @"INSERT INTO mastervariantcogscalc(@PwShopId) 
                SELECT t2.PwMasterVariantId, @PwShopId, @StartDate, @EndDate, 
                        @PercentMultiplier, @SourceCurrencyId, @FixedAmount
                FROM picklistmasterproduct(@PwShopId) t1  
	                INNER JOIN mastervariant(@PwShopId) t2 ON t1.PwMasterProductId = t2.PwMasterProductId
                WHERE t1.PwPickListId = @PwPickListId ";

            var calcContext = context.ToCalcContext();

            _connectionWrapper.Execute(
                query, new
                {
                    PwShop.PwShopId,
                    context.PwPickListId,
                    context.StartDate,
                    context.EndDate,
                    calcContext.PercentMultiplier,
                    calcContext.SourceCurrencyId,
                    calcContext.FixedAmount,
                });
        }


        public void DeleteCogsCalcByMasterVariant(CogsDateBlockContext context)
        {
            var query = @"DELETE FROM mastervariantcogscalc(@PwShopId) 
                        WHERE PwMasterVariantId = @PwMasterVariantId";
            _connectionWrapper.Execute(query, new { PwShop.PwShopId, context.PwMasterVariantId });
        }

        public void DeleteCogsCalcByMasterProduct(CogsDateBlockContext context)
        {
            var query =
                @"DELETE FROM mastervariantcogscalc(@PwShopId)
                WHERE PwMasterVariantId IN ( 
                    SELECT PwMasterVariantId FROM mastervariant(@PwShopId)
                    WHERE PwMasterProductId = @PwMasterProductId )";

            _connectionWrapper.Execute(query, new { PwShop.PwShopId, context.PwMasterProductId });
        }

        public void DeleteCogsCalcByPickList(long pwPickListId)
        {
            var query =
                @"DELETE FROM mastervariantcogscalc(@PwShopId) 
                WHERE PwMasterVariantId IN
                (
	                SELECT t2.PwMasterVariantId
	                FROM picklistmasterproduct(@PwShopId) t1 
		                INNER JOIN mastervariant(@PwShopId) t2 
                            ON t1.PwMasterProductId = t2.PwMasterProductId
	                WHERE t1.PwPickListId = @pwPickListId
                )";

            _connectionWrapper.Execute(query, new { PwShop.PwShopId, pwPickListId });
        }
    }
}

