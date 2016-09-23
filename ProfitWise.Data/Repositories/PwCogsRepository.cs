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

        public long InsertQuery()
        {
            var query =
                @"INSERT INTO profitwisequery ( PwShopId )
                    VALUES ( @PwShopId );
                    SELECT LAST_INSERT_ID();";

            return _connection.Query<long>(
                query, new {PwShopId = this.PwShop.PwShopId}).FirstOrDefault();
        }

        public void InsertMasterProductPickList(long queryId, List<string> terms)
        {
            var query =
                @"INSERT INTO profitwisequerymasterproduct (PwQueryId, PwShopId, PwMasterProductId)
                SELECT DISTINCT @QueryId, @PwShopId, t2.PwMasterProductId
                FROM profitwisevariant t1 
	                INNER JOIN profitwisemastervariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN profitwiseproduct t3 ON t2.PwMasterProductId = t3.PwMasterProductId
                WHERE (t1.PwShopId = @PwShopId AND t2.PwShopId = @PwShopId AND t3.PwShopId = @PwShopId)";

            foreach (var term in terms)
            {
                var clause =
                    $" AND ( (t1.Title LIKE '%{term}%') OR (t1.Sku LIKE '%{term}%') OR ( t3.Title LIKE '%{term}%' ) OR ( t3.Vendor LIKE '%{term}%' ) )";
                query += clause;
            }
            _connection.Execute(query, new { QueryId = queryId, PwShopId = this.PwShop.PwShopId});
        }

        public IList<PwCogsProductSearchResult> RetrieveMasterProducts(long queryId, int pageNumber, int resultsPerPage)
        {
            if (resultsPerPage > 200)
            {
                throw new ArgumentException("Maximum number of results per page is 200");
            }

            var startRecord = (pageNumber - 1) * resultsPerPage;

            var query =
                @"SELECT t1.PwMasterProductId, t1.PwProductId, t1.Title, t1.Vendor
                FROM profitwiseproduct t1
                WHERE t1.PwShopId = @PwShopId AND t1.IsPrimary = true
                AND t1.PwMasterProductId IN ( 
	                SELECT PwMasterProductId FROM profitwisequerymasterproduct WHERE PwShopId = @PwShopId AND PwQueryId = @QueryId )
                ORDER BY t1.Title ASC
                LIMIT @StartRecord, @ResultsPerPage;";

            return _connection.Query<PwCogsProductSearchResult>(
                query,
                new
                {
                    PwShopId = this.PwShop.PwShopId,
                    QueryId = queryId,
                    StartRecord = startRecord,
                    ResultsPerPage = resultsPerPage
                }).ToList();
        }

        /// <summary>
        /// Note: cannot handle more than 200 Master Product Ids
        /// </summary>
        public IList<PwCogsVariantSearchResult> RetrieveMasterVariants(IList<long> masterProductIds)
        {
            var query =
                @"SELECT t2.PwMasterProductId, t2.PwMasterVariantId, t2.Exclude, t2.StockedDirectly, 
                    t2.CogsCurrencyId, t2.CogsAmount, t2.CogsDetail, t3.PwVariantId, 
                    t3.LowPrice, t3.HighPrice, t3.Inventory
                FROM profitwisemastervariant t2 
	                INNER JOIN profitwisevariant t3 ON t2.PwMasterVariantId = t3.PwMasterVariantId
                WHERE t2.PwShopId = @PwShopId
                AND t3.PwShopId = @PwShopId AND t3.IsPrimary = true
                AND t2.PwMasterProductId IN @MasterProductIds";

            return _connection.Query<PwCogsVariantSearchResult>(
                query, new {PwShopId = this.PwShop.PwShopId, MasterProductIds = masterProductIds}).ToList();
        }
    }
}
