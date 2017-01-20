﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwCogsRepository : IShopFilter
    {
        private readonly MySqlConnection _connection;

        public PwCogsRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;


        public MySqlTransaction InitiateTransaction()
        {
            return _connection.BeginTransaction();
        }

        public PwCogsProductSummary RetrieveProduct(long masterProductId)
        {
            var query =
                @"SELECT PwMasterProductId, PwProductId, Title, Vendor
                FROM profitwiseproduct
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId = @PwMasterProductId
                AND IsPrimary = true;";

            return _connection
                .Query<PwCogsProductSummary>(
                    query, new
                    {
                        PwShopId = this.PwShop.PwShopId,
                        PwMasterProductId = masterProductId,
                    })
                .FirstOrDefault();
        }

        public IList<PwCogsProductSummary> RetrieveProductsFromPicklist(
                long pickListId, int pageNumber, int resultsPerPage, int sortByColumn, bool sortByDirectionDown)
        {
            if (resultsPerPage > 200)
            {
                throw new ArgumentException("Maximum number of results per page is 200");
            }

            var startRecord = (pageNumber - 1) * resultsPerPage;
            var sortDirectionWord = (sortByDirectionDown ? "ASC" : "DESC");

            var sortByClause =
                "ORDER BY " +
                (sortByColumn == 0
                    ? $"t1.Vendor {sortDirectionWord}, t1.Title {sortDirectionWord} "
                    : $"t1.Title {sortDirectionWord}, t1.Vendor {sortDirectionWord} ");

            var query =
                @"SELECT t1.PwMasterProductId, t1.PwProductId, t1.Title, t1.Vendor
                FROM profitwiseproduct t1
                    INNER JOIN profitwisemasterproduct t2
                        ON t1.PwMasterProductId = t2.PwMasterProductId
                WHERE t1.PwShopId = @PwShopId AND t1.IsPrimary = true AND t2.PwShopId = @PwShopId
                AND t1.PwMasterProductId IN ( 
	                SELECT PwMasterProductId FROM profitwisepicklistmasterproduct 
                    WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId ) " +
                sortByClause + " LIMIT @StartRecord, @ResultsPerPage;";

            return _connection.Query<PwCogsProductSummary>(
                query,
                new
                {
                    PwShopId = this.PwShop.PwShopId,
                    pickListId,
                    StartRecord = startRecord,
                    ResultsPerPage = resultsPerPage
                }).ToList();
        }

        /// <summary>
        /// Note: cannot handle more than 200 Master Product Ids i.e. paging should not exceed that
        /// TODO - the High and Low computations are incorrect here
        /// </summary>
        public IList<PwCogsVariant> RetrieveVariants(IList<long> masterProductIds)
        {
            var query =
                @"SELECT t2.PwMasterProductId, t2.PwMasterVariantId, t3.Title, t3.Sku, t2.Exclude, t2.StockedDirectly, 
                        t2.CogsTypeId, t2.CogsPercentage, t2.CogsCurrencyId, t2.CogsAmount, t2.CogsDetail, 
                        t3.PwVariantId, t3.LowPrice, t3.HighPrice, t3.Inventory
                FROM profitwisemastervariant t2 
	                INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
                WHERE t2.PwShopId = @PwShopId
                AND t3.PwShopId = @PwShopId AND t3.IsPrimary = true
                AND t2.PwMasterProductId IN @MasterProductIds";

            return _connection.Query<PwCogsVariant>(
                query, new {this.PwShopId, MasterProductIds = masterProductIds}).ToList();
        }

        // TODO - the High and Low computations are incorrect here
        public PwCogsVariant RetrieveVariant(long masterVariantId)
        {
            var query =
                @"SELECT t2.PwMasterProductId, t2.PwMasterVariantId, t3.Title, t3.Sku, t2.Exclude, t2.StockedDirectly, 
                        t2.CogsTypeId, t2.CogsPercentage, t2.CogsCurrencyId, t2.CogsAmount, t2.CogsDetail, 
                        t3.PwVariantId, t3.LowPrice, t3.HighPrice, t3.Inventory
                FROM profitwisemastervariant t2 
                        INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
	            WHERE t2.PwShopId = @PwShopId
                AND t3.PwShopId = @PwShopId AND t3.IsPrimary = true
                AND t2.PwMasterVariantId = @masterVariantId";

            return _connection
                .Query<PwCogsVariant>(query, new { this.PwShopId, masterVariantId })
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

            _connection.Execute(query, new {PwShopId = this.PwShop.PwShopId, pickListId, stockedDirectly});
        }

        public void UpdateStockedDirectlyByMasterProductId(long masterProductId, bool stockedDirectly)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET StockedDirectly = @stockedDirectly
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId = @masterProductId;";

            _connection.Execute(query, new {PwShopId = this.PwShop.PwShopId, masterProductId, stockedDirectly});
        }

        public void UpdateStockedDirectlyByMasterVariantId(long masterVariantId, bool stockedDirectly)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET StockedDirectly = @stockedDirectly
                WHERE PwShopId = @PwShopId
                AND PwMasterVariantId = @masterVariantId;";

            _connection.Execute(query, new {PwShopId = this.PwShop.PwShopId, masterVariantId, stockedDirectly});
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

            _connection.Execute(query, new {PwShopId, pickListId, exclude});
        }

        public void UpdateExcludeByMasterProductId(long masterProductId, bool exclude)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET Exclude = @exclude
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId = @masterProductId;";

            _connection.Execute(query, new {PwShopId, masterProductId, exclude});
        }

        public void UpdateExcludeByMasterVariantId(long masterVariantId, bool exclude)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET Exclude = @exclude
                WHERE PwShopId = @PwShopId
                AND PwMasterVariantId = @masterVariantId;";

            _connection.Execute(query, new {PwShopId, masterVariantId, exclude});
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

            return _connection.Query<string>(
                query, new {PwShopId = this.PwShop.PwShopId}).ToList();
        }

        public IList<string> RetrieveProductType()
        {
            var query = @"SELECT DISTINCT ProductType AS ProductType
                        FROM profitwiseproduct 
                        WHERE PwShopId = @PwShopId
                        AND ProductType IS NOT NULL
                        AND ProductType <> ''
                        ORDER BY ProductType;";

            return _connection.Query<string>(
                query, new {PwShopId = this.PwShop.PwShopId}).ToList();
        }



        // Master Variant Cogs Defaults
        // TODO: this will be overhauled...
        public void UpdateDefaultCogs(
                long? masterVariantId, int cogsTypeId, int? cogsCurrencyId, decimal? cogsAmount, 
                decimal? cogsPercentage, bool cogsDetail)
        {
            var query =
                @"UPDATE profitwisemastervariant
                SET CogsCurrencyId = @cogsCurrencyId, 
                    CogsAmount = @cogsAmount,
                    CogsTypeId = @cogsTypeId,
                    CogsPercentage = @cogsPercentage,
                    CogsDetail = @cogsDetail                   
                WHERE PwShopId = @PwShopId AND PwMasterVariantId = @masterVariantId;";

            _connection.Execute(
                query, new { this.PwShopId, masterVariantId,
                            cogsTypeId, cogsCurrencyId, cogsAmount, cogsPercentage, cogsDetail });
        }

        public void UpdateProductCogsAllVariants(long masterProductId, int currencyId, decimal amount)
        {
            var query = @"UPDATE profitwisemastervariant
                        SET CogsAmount = @amount, CogsCurrencyId = @currencyId
                        WHERE PwShopId = @PwShopId
                        AND PwMasterProductId = @masterProductId;";

            _connection.Execute(query,
                new { this.PwShopId, masterProductId, currencyId, amount });
        }

        public void BulkUpdateMasterVariantCogsToDefault(bool zeroCogsOnly)
        {
            var query =
                @"UPDATE profitwiseshop t0
                    INNER JOIN profitwisemastervariant t1 
		                ON t0.PwShopId = t1.PwShopId
	                INNER JOIN profitwisevariant t2 
		                ON t1.PwShopId = t2.PwShopId 
                            AND t1.PwMasterVariantId = t2.PwMasterVariantId 
                            AND t2.IsPrimary = 1
                SET t1.CogsCurrencyId = t0.CurrencyId, 
                    t1.CogsAmount = t2.HighPrice * @FractionOfHighPrice, 
                    t1.CogsDetail = false
                WHERE t0.PwShopId = @PwShopId ";

            if (zeroCogsOnly)
            {
                query += "AND t1.CogsAmount IS NULL;";
            }

            var fractionOfHighPrice = 1.00m - this.PwShop.DefaultMargin;
            _connection.Execute(query, new {this.PwShopId, FractionOfHighPrice = fractionOfHighPrice});
        }


        // CoGS Detail functions
        public List<PwCogsDetail> RetrieveCogsDetail(long? masterVariantId)
        {
            var query =
                @"SELECT * FROM profitwisemastervariantcogsdetail 
                WHERE PwShopId = @PwShopId AND PwMasterVariantId = @masterVariantId
                ORDER BY CogsDate;";

            return _connection.Query<PwCogsDetail>(
                query, new { this.PwShop.PwShopId, @masterVariantId }).ToList();
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
            _connection.Execute(query, new {this.PwShopId, masterVariantId});
        }

        public void InsertCogsDetails(PwCogsDetail detail)
        {
            detail.PwShopId = this.PwShopId;
            var query = 
                @"INSERT INTO profitwisemastervariantcogsdetail 
                VALUES ( @PwMasterVariantId, @PwShopId, 
                        @CogsDate, @CogsTypeId, @CogsCurrencyId, @CogsAmount, @CogsPercentage );";
            _connection.Execute(query, detail);
        }



        // Order Line CoGS propagation functions...
        public void UpdateOrderLineUnitCogs(
                bool onlyNullUnitCogs, long? masterProductId = null, long? masterVariantId = null)
        {
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
                SET t3.UnitCogs = (t1.CogsAmount * IFNULL(t4.Rate, 0))
                WHERE t0.PwShopId = 100001 ";

            if (onlyNullUnitCogs)
            {
                query += "AND t3.UnitCogs IS NULL ";
            }
            if (masterProductId.HasValue)
            {
                query += "AND t1.PwMasterProductId = @PwMasterProductId ";
            }
            if (masterVariantId.HasValue)
            {
                query += "AND t1.PwMasterVariantId = @PwMasterVariantId ";
            }

            _connection.Execute(
                query, new { PwShopId, PwMasterProductId = masterProductId, PwMasterVariantId = masterVariantId });
        }
    }
}

