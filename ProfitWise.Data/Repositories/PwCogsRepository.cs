using System;
using System.Collections.Generic;
using System.Data.Common;
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


        public MySqlTransaction InitiateTransaction()
        {
            return _connection.BeginTransaction();
        }

        public long DeletePickList()
        {
            var query =
                @"DELETE FROM profitwisequerymasterproduct WHERE PwShopId = @PwShopId;";

            return _connection.Query<long>(
                query, new {PwShopId = this.PwShop.PwShopId}).FirstOrDefault();
        }

        public int InsertPickList(List<string> terms)
        {
            var query =
                @"INSERT INTO profitwisequerymasterproduct (PwShopId, PwMasterProductId)
                SELECT DISTINCT @PwShopId, t2.PwMasterProductId
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

            query += "; SELECT COUNT(*) FROM profitwisequerymasterproduct " +
                     "WHERE PwShopId = @PwShopId";
            
            return _connection
                    .Query<int>(query, new {PwShopId = this.PwShop.PwShopId})
                    .First();
        }
        

        public IList<PwCogsProductSearchResult> 
                RetrieveMasterProducts(int pageNumber, int resultsPerPage, int sortByColumn, bool sortByDirectionDown)
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
	                SELECT PwMasterProductId FROM profitwisequerymasterproduct WHERE PwShopId = @PwShopId ) " +
                sortByClause +
                " LIMIT @StartRecord, @ResultsPerPage;";

            return _connection.Query<PwCogsProductSearchResult>(
                query,
                new
                {
                    PwShopId = this.PwShop.PwShopId,
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
    }
}
