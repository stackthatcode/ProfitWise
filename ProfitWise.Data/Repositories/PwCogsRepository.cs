using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

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

        public IList<PwCogsProductSummary> 
                RetrieveProductsFromPicklist(long pickListId, int pageNumber, int resultsPerPage, int sortByColumn, bool sortByDirectionDown)
        {
            if (resultsPerPage > 200)
            {
                throw new ArgumentException("Maximum number of results per page is 200");
            }

            var startRecord = (pageNumber - 1) * resultsPerPage;

            var sortDirectionWord= (sortByDirectionDown ? "ASC" : "DESC");

            var sortByClause =
                "ORDER BY " +
                (sortByColumn == 0
                    ? $"t1.Vendor {sortDirectionWord}, t1.Title {sortDirectionWord} "
                    : $"t1.Title {sortDirectionWord}, t1.Vendor {sortDirectionWord} ");
                    
            var query =
                @"SELECT t1.PwMasterProductId, t1.PwProductId, t1.Title, t1.Vendor
                FROM profitwiseproduct t1
                WHERE t1.PwShopId = @PwShopId AND t1.IsPrimary = true
                AND t1.PwMasterProductId IN ( 
	                SELECT PwMasterProductId FROM profitwisepicklistmasterproduct 
                    WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId  ) " +
                sortByClause +
                " LIMIT @StartRecord, @ResultsPerPage;";

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
        /// Note: cannot handle more than 200 Master Product Ids
        /// </summary>
        public IList<PwCogsVariant> RetrieveVariants(IList<long> masterProductIds)
        {
            var query =
                @"SELECT t2.PwMasterProductId, t2.PwMasterVariantId, t3.Title, t3.Sku, t2.Exclude, t2.StockedDirectly, 
                    t2.CogsCurrencyId, t2.CogsAmount, t2.CogsDetail, t3.PwVariantId, 
                    t3.LowPrice, t3.HighPrice, t3.Inventory
                FROM profitwisemastervariant t2 
	                INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
                WHERE t2.PwShopId = @PwShopId
                AND t3.PwShopId = @PwShopId AND t3.IsPrimary = true
                AND t2.PwMasterProductId IN @MasterProductIds";

            return _connection.Query<PwCogsVariant>(
                query, new { this.PwShopId, MasterProductIds = masterProductIds }).ToList();
        }

        public void UpdateProductCogsAllVariants(long masterProductId, int currencyId, decimal amount)
        {
            var query = @"UPDATE profitwisemastervariant
                        SET CogsAmount = @amount, CogsCurrencyId = @currencyId
                        WHERE PwShopId = @PwShopId
                        AND PwMasterProductId = @masterProductId;";

            _connection.Execute(query, 
                new { this.PwShopId, masterProductId, currencyId, amount});
        }

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

        public void UpdateStockedDirectlyById(long masterProductId, bool stockedDirectly)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET StockedDirectly = @stockedDirectly
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId = @masterProductId;";

            _connection.Execute(query, new {PwShopId = this.PwShop.PwShopId, masterProductId, stockedDirectly });
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

            _connection.Execute(query, new { PwShopId, pickListId, exclude });
        }

        public void UpdateExcludeById(long masterProductId, bool exclude)
        {
            var query =
                @"UPDATE profitwisemastervariant 
                SET Exclude = @exclude
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId = @masterProductId;";

            _connection.Execute(query, new { PwShopId, masterProductId, exclude });
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
                query, new { PwShopId = this.PwShop.PwShopId }).ToList();
        }



        // Master Variant Cogs entry
        public void UpdateMasterVariantCogs(long masterVariantId, int currencyId, decimal amount)
        {
            var query =
                @"UPDATE profitwisemastervariant
                SET CogsCurrencyId = @currencyId, CogsAmount = @amount
                WHERE PwShopId = @PwShopId AND PwMasterVariantId = @masterVariantId;";
            _connection.Execute(
                query, new { this.PwShopId, masterVariantId, currencyId, amount });
        }
    }
}
