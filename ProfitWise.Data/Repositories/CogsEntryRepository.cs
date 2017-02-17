using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class CogsEntryRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;

        public CogsEntryRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }

        public PwCogsProductSummary RetrieveProduct(long masterProductId)
        {
            var query =
                @"SELECT PwMasterProductId, PwProductId, Title, Vendor
                FROM profitwiseproduct
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId = @PwMasterProductId
                AND IsPrimary = 1;";

            return Connection
                .Query<PwCogsProductSummary>(
                    query, new
                    {
                        PwShopId = this.PwShop.PwShopId,
                        PwMasterProductId = masterProductId,
                    }, _connectionWrapper.Transaction)
                .FirstOrDefault();
        }
        
        public void TouchPickList(long pickListId)
        {
            var touchQuery =
                @"UPDATE profitwisepicklist SET LastAccessed = getdate() 
                WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId";
            Connection.Execute(touchQuery, new { PwShopId, pickListId }, _connectionWrapper.Transaction);
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
                FROM profitwiseproduct WHERE PwShopId = @PwShopId " + 
                isPrimaryClause +
                @"AND PwMasterProductId IN ( 
	                SELECT PwMasterProductId FROM profitwisepicklistmasterproduct 
                    WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId ) " +
                sortByClause + " OFFSET @StartRecord ROWS FETCH NEXT @ResultsPerPage ROWS ONLY;";
            
            // sortByClause + " LIMIT @StartRecord, @ResultsPerPage;"; //MySQL

            return query;
        }

        // Sort By Column { 0 for Vendor, 1 for Product Title }
        public IList<PwCogsProductSummary> RetrieveCogsSummaryFromPicklist(
                long pickListId, int pageNumber, int resultsPerPage, 
                int sortByColumn = 0, bool sortByDirectionDown = true)
        {
            if (resultsPerPage > 200)
            {
                throw new ArgumentException("Maximum number of results per page is 200");
            }

            TouchPickList(pickListId);
            var query = ResultsQueryGen(sortByColumn, sortByDirectionDown);
            var startRecord = (pageNumber - 1) * resultsPerPage;

            return Connection.Query<PwCogsProductSummary>(
                query, new { this.PwShop.PwShopId, pickListId, StartRecord = startRecord, ResultsPerPage = resultsPerPage },
                _connectionWrapper.Transaction).ToList();
        }
        

        // TODO - the High and Low computations are incorrect here
        public IList<PwCogsVariant> RetrieveVariants(IList<long> masterProductIds)
        {
            var query =
                @"SELECT t2.PwMasterProductId, t2.PwMasterVariantId, t3.Title, t4.Title AS ProductTitle, t3.Sku, 
                        t2.Exclude, t2.StockedDirectly, t2.CogsTypeId, t2.CogsMarginPercent, t2.CogsCurrencyId, 
                        t2.CogsAmount, t2.CogsDetail, t3.PwVariantId, t3.LowPrice, t3.HighPrice, t3.Inventory
                FROM profitwisemastervariant t2 
	                INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
                    INNER JOIN profitwiseproduct t4 ON t3.PwProductId = t4.PwProductId
                WHERE t2.PwShopId = @PwShopId
                AND t3.PwShopId = @PwShopId AND t3.IsPrimary = 1
                AND t4.PwShopId = @PwShopId
                AND t2.PwMasterProductId IN @MasterProductIds";

            return Connection.Query<PwCogsVariant>(
                query, new {this.PwShopId, MasterProductIds = masterProductIds},
                _connectionWrapper.Transaction).ToList();
        }

        // TODO - the High and Low computations are incorrect here
        public PwCogsVariant RetrieveVariant(long masterVariantId)
        {
            var query =
                @"SELECT t2.PwMasterProductId, t2.PwMasterVariantId, t3.Title, t3.Sku, t2.Exclude, t2.StockedDirectly, 
                        t2.CogsTypeId, t2.CogsMarginPercent, t2.CogsCurrencyId, t2.CogsAmount, t2.CogsDetail, 
                        t3.PwVariantId, t3.LowPrice, t3.HighPrice, t3.Inventory
                FROM profitwisemastervariant t2 
                        INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
	            WHERE t2.PwShopId = @PwShopId
                AND t3.PwShopId = @PwShopId AND t3.IsPrimary = 1
                AND t2.PwMasterVariantId = @masterVariantId";

            return Connection
                .Query<PwCogsVariant>(
                     query, new { this.PwShopId, masterVariantId }, _connectionWrapper.Transaction)
                .FirstOrDefault();
        }        


        // Stocked Directly and Exclude data
        public void UpdateStockedDirectlyByPicklist(long pickListId, bool stockedDirectly)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET StockedDirectly = @stockedDirectly
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId IN
                    (SELECT PwMasterProductId 
                    FROM profitwisepicklistmasterproduct 
                    WHERE PwShopId = @pwShopId AND PwPickListId = @pickListId);";

            Connection.Execute(
                query, new {PwShopId = this.PwShop.PwShopId, pickListId, stockedDirectly}, _connectionWrapper.Transaction);
        }

        public void UpdateStockedDirectlyByMasterProductId(long masterProductId, bool stockedDirectly)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET StockedDirectly = @stockedDirectly
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId = @masterProductId;";

            Connection.Execute(
                query, new {PwShopId = this.PwShop.PwShopId, masterProductId, stockedDirectly}, 
                _connectionWrapper.Transaction);
        }

        public void UpdateStockedDirectlyByMasterVariantId(long masterVariantId, bool stockedDirectly)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET StockedDirectly = @stockedDirectly
                WHERE PwShopId = @PwShopId
                AND PwMasterVariantId = @masterVariantId;";

            Connection.Execute(
                query, new {PwShopId = this.PwShop.PwShopId, masterVariantId, stockedDirectly}, 
                _connectionWrapper.Transaction);
        }

        public void UpdateExcludeByPicklist(long pickListId, bool exclude)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET Exclude = @exclude
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId IN
                    (SELECT PwMasterProductId 
                    FROM profitwisepicklistmasterproduct 
                    WHERE PwShopId = @pwShopId AND PwPickListId = @pickListId);";

            Connection.Execute(query, new {PwShopId, pickListId, exclude}, _connectionWrapper.Transaction);
        }

        public void UpdateExcludeByMasterProductId(long masterProductId, bool exclude)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET Exclude = @exclude
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId = @masterProductId;";

            Connection.Execute(query, new {PwShopId, masterProductId, exclude}, _connectionWrapper.Transaction);
        }

        public void UpdateExcludeByMasterVariantId(long masterVariantId, bool exclude)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET Exclude = @exclude
                WHERE PwShopId = @PwShopId
                AND PwMasterVariantId = @masterVariantId;";

            Connection.Execute(query, new {PwShopId, masterVariantId, exclude}, _connectionWrapper.Transaction);
        }


        // Product Search input
        public IList<string> RetrieveVendors()
        {
            var query = @"SELECT DISTINCT Vendor AS Vendor
                        FROM profitwiseproduct 
                        WHERE PwShopId = @PwShopId
                        AND Vendor IS NOT NULL
                        AND Vendor <> ''
                        ORDER BY Vendor;";

            return Connection.Query<string>(
                query, new {PwShopId = this.PwShop.PwShopId}, _connectionWrapper.Transaction).ToList();
        }

        public IList<string> RetrieveProductType()
        {
            var query = @"SELECT DISTINCT ProductType AS ProductType
                        FROM profitwiseproduct 
                        WHERE PwShopId = @PwShopId
                        AND ProductType IS NOT NULL
                        AND ProductType <> ''
                        ORDER BY ProductType;";

            return Connection.Query<string>(
                query, new {PwShopId = this.PwShop.PwShopId}, _connectionWrapper.Transaction).ToList();
        }


        // Master Variant Cogs Defaults
        public void UpdateMasterVariantDefaultCogs(PwCogsDetail input, bool hasDetail)
        {
            UpdateMasterVariantDefaultCogs(input.PwMasterVariantId, input.CogsTypeId, input.CogsCurrencyId,
                    input.CogsAmount, input.CogsMarginPercent, hasDetail);
        } 
       
        public void UpdateMasterVariantDefaultCogs(
                long? masterVariantId, int cogsTypeId, int? cogsCurrencyId, decimal? cogsAmount, 
                decimal? cogsMarginPercent, bool cogsDetail)
        {
            if (cogsTypeId != CogsType.FixedAmount && cogsTypeId != CogsType.MarginPercentage)
            {
                throw new ArgumentException("Invalid cogstypeId");
            }

            var query =
                @"UPDATE profitwisemastervariant
                SET CogsCurrencyId = @cogsCurrencyId, 
                    CogsAmount = @cogsAmount,
                    CogsTypeId = @cogsTypeId,
                    CogsMarginPercent = @cogsMarginPercent,
                    CogsDetail = @cogsDetail                   
                WHERE PwShopId = @PwShopId AND PwMasterVariantId = @masterVariantId;";

            Connection.Execute(
                query, new { this.PwShopId, masterVariantId,
                            cogsTypeId, cogsCurrencyId, cogsAmount, cogsMarginPercent, cogsDetail }, 
                _connectionWrapper.Transaction);
        }

        public void UpdatePickListDefaultCogs(long pickListId, CogsDto input)
        {
            var query =
                @"UPDATE t1        
                SET CogsCurrencyId = @CogsCurrencyId, 
                    CogsAmount = @CogsAmount,
                    CogsTypeId = @CogsTypeId,
                    CogsMarginPercent = @CogsMarginPercent,
                    CogsDetail = @CogsDetail   
                FROM profitwisemastervariant t1
                    INNER JOIN profitwisepicklistmasterproduct t2
                        ON t1.PwMasterProductId = t2.PwMasterProductId                               
                WHERE t1.PwShopId = @PwShopId 
                AND t2.PwShopId = @PwShopId 
                AND PwPickListId = @pickListId;";

            Connection.Execute(
                query, new
                {
                    this.PwShopId,
                    pickListId,
                    input.CogsTypeId,
                    input.CogsCurrencyId,
                    input.CogsAmount,
                    input.CogsMarginPercent,
                    CogsDetail = false,
                }, _connectionWrapper.Transaction);
        }


        // CoGS Detail functions
        public List<PwCogsDetail> RetrieveCogsDetailByMasterVariant(long? masterVariantId)
        {
            var query =
                @"SELECT * FROM profitwisemastervariantcogsdetail 
                WHERE PwShopId = @PwShopId AND PwMasterVariantId = @masterVariantId
                ORDER BY CogsDate;";

            return Connection.Query<PwCogsDetail>(
                query, new { this.PwShop.PwShopId, @masterVariantId }, _connectionWrapper.Transaction).ToList();
        }

        public List<PwCogsDetail> RetrieveCogsDetailByMasterProduct(long? masterProductId)
        {
            var query =
                @"SELECT t1.* FROM profitwisemastervariantcogsdetail t1
                        INNER JOIN profitwisemastervariant t2 
                            ON t1.PwShopId = t2.PwShopId 
                            AND t1.PwMasterVariantId = t2.PwMasterVariantId
                WHERE t1.PwShopId = @PwShopId AND t2.PwMasterProductId = @masterProductId
                ORDER BY t1.CogsDate;";

            return Connection.Query<PwCogsDetail>(
                query, new { this.PwShop.PwShopId, masterProductId }, _connectionWrapper.Transaction).ToList();
        }

        public List<PwCogsDetail> RetrieveCogsDetailAll()
        {
            var query =
                @"SELECT * FROM profitwisemastervariantcogsdetail 
                WHERE PwShopId = @PwShopId ORDER BY CogsDate;";

            return Connection.Query<PwCogsDetail>(query, new { this.PwShop.PwShopId }, _connectionWrapper.Transaction).ToList();
        }

        public void DeleteCogsDetail(long? masterVariantId)
        {
            if (!masterVariantId.HasValue)
            {
                throw new ArgumentNullException();
            }

            var query = 
                @"DELETE FROM profitwisemastervariantcogsdetail 
                WHERE PwShopId = @PwShopId AND PwMasterVariantId = @masterVariantId;";
            Connection.Execute(query, new {this.PwShopId, masterVariantId}, _connectionWrapper.Transaction);
        }

        public void InsertCogsDetails(PwCogsDetail detail)
        {
            if (detail.CogsTypeId != CogsType.FixedAmount && detail.CogsTypeId != CogsType.MarginPercentage)
            {
                throw new ArgumentException("Invalid CogsTypeId");
            }
            detail.PwShopId = this.PwShopId;
            var query = 
                @"INSERT INTO profitwisemastervariantcogsdetail 
                VALUES ( @PwMasterVariantId, @PwShopId, 
                        @CogsDate, @CogsTypeId, @CogsCurrencyId, @CogsAmount, @CogsMarginPercent );";
            Connection.Execute(query, detail, _connectionWrapper.Transaction);
        }



        // Goods on Hand queries
        public void InsertCogsCalcByMasterVariant(CogsDateBlockContext context)
        {
            var query =
                @"INSERT INTO profitwisemastervariantcogscalc 
                VALUES (
                    @PwMasterVariantId, @PwShopId, @StartDate, @EndDate, 
                    @PercentMultiplier, @SourceCurrencyId, @FixedAmount  )";

            var calcContext = context.ToCalcContext();

            Connection.Execute(
                query, new
                {
                    PwShop.PwShopId,
                    context.PwMasterVariantId,
                    context.StartDate,
                    context.EndDate,
                    calcContext.PercentMultiplier,
                    calcContext.SourceCurrencyId,
                    calcContext.FixedAmount,
                },
                _connectionWrapper.Transaction);
        }

        public void InsertCogsCalcByMasterProduct(CogsDateBlockContext context)
        {
            var query =
                @"INSERT INTO profitwisemastervariantcogscalc 
                SELECT PwMasterVariantId, @PwShopId, @StartDate, @EndDate, 
                        @PercentMultiplier, @SourceCurrencyId, @FixedAmount
                FROM profitwisemastervariant 
                WHERE PwMasterProductId = @PwMasterProductId
                AND PwShopId = @PwShopId";

            var calcContext = context.ToCalcContext();

            Connection.Execute(
                query, new {
                    PwShop.PwShopId, context.PwMasterProductId, context.StartDate, context.EndDate,
                    calcContext.PercentMultiplier, calcContext.SourceCurrencyId, calcContext.FixedAmount,
                },
                _connectionWrapper.Transaction);
        }

        public void InsertCogsCalcByPickList(CogsDateBlockContext context)
        {
            var query =
                @"INSERT INTO profitwisemastervariantcogscalc 
                SELECT t2.PwMasterVariantId, @PwShopId, @StartDate, @EndDate, 
                        @PercentMultiplier, @SourceCurrencyId, @FixedAmount
                FROM profitwisepicklistmasterproduct t1  
	                INNER JOIN profitwisemastervariant t2 ON t1.PwMasterProductId = t2.PwMasterProductId
                WHERE t1.PwShopId = @PwShopId 
	                AND t2.PwShopId = @PwShopId
	                AND t1.PwPickListId = @PwPickListId ";

            var calcContext = context.ToCalcContext();

            Connection.Execute(
                query, new
                {
                    PwShop.PwShopId,
                    context.PwPickListId,
                    context.StartDate,
                    context.EndDate,
                    calcContext.PercentMultiplier,
                    calcContext.SourceCurrencyId,
                    calcContext.FixedAmount,
                },
                _connectionWrapper.Transaction);
        }

        public void DeleteCogsCalcByMasterVariant(CogsDateBlockContext context)
        {
            var query =
                @"DELETE FROM profitwisemastervariantcogscalc
                WHERE PwMasterVariantId = @PwMasterVariantId
                AND PwShopId = @PwShopId";

            Connection.Execute(
                query, new { PwShop.PwShopId, context.PwMasterVariantId },
                _connectionWrapper.Transaction);
        }

        public void DeleteCogsCalcByMasterProduct(CogsDateBlockContext context)
        {
            var query =
                @"DELETE FROM profitwisemastervariantcogscalc
                WHERE PwMasterVariantId IN ( 
                    SELECT PwMasterVariantId FROM profitwisemastervariant
                    WHERE PwShopId = @PwShopID AND PwMasterProductId = @PwMasterProductId )
                AND PwShopId = @PwShopId";

            Connection.Execute(
                query, new { PwShop.PwShopId, context.PwMasterProductId },
                _connectionWrapper.Transaction);
        }

        public void DeleteCogsCalcByPickList(CogsDateBlockContext context)
        {
            var query =
                @"DELETE FROM profitwisemastervariantcogscalc 
                WHERE PwMasterVariantId IN
                (
	                SELECT t2.PwMasterVariantId
	                FROM profitwisepicklistmasterproduct t1 
		                INNER JOIN profitwisemastervariant t2 ON t1.PwMasterProductId = t2.PwMasterProductId
	                WHERE t1.PwShopId = @PwShopId 
	                AND t2.PwShopId = @PwShopId
	                AND t1.PwPickListId = @PwPickListId
                )
                AND PwShopId  = @PwShopId;";

            Connection.Execute(
                query, new { PwShop.PwShopId, context.PwPickListId }, _connectionWrapper.Transaction);
        }

    }
}

