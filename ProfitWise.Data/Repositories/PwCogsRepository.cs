using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Helpers;

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


        // Pick List provisioning operation
        public long ProvisionPickList()
        {
            var query =
                @"INSERT INTO profitwisepicklist (PwShopId, CreatedDate) VALUES (@PwShopId, @createdDate);
                SELECT LAST_INSERT_ID();";

            return _connection.Query<long>(query, 
                    new { PwShopId = this.PwShop.PwShopId, createdDate = DateTime.Now }).First();
        }

        public long DecommissionPickList(long pickListId)
        {
            var query =
                @"DELETE FROM profitwisepicklist
                WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId;";

            return _connection.Query<long>(
                query, new {PwShopId = this.PwShop.PwShopId, pickListId}).FirstOrDefault();
        }

        public long DecommissionPickList(DateTime cutoffDate)
        {
            var query =
                @"DELETE FROM profitwisepicklist
                WHERE PwShopId = @PwShopId AND CreatedDate <= @cutoffDate;";

            return _connection.Query<long>(
                query, new { PwShopId = this.PwShop.PwShopId, cutoffDate }).FirstOrDefault();
        }



        // Pick List population/filter operations
        public void PopulatePickList(long pickListId, List<string> searchTerms)
        {
            var query =
                @"INSERT INTO profitwisepicklistmasterproduct (PwShopId, PwMasterProductId)
                SELECT DISTINCT @PwShopId, @PwPickListId, t2.PwMasterProductId
                FROM profitwisevariant t1 
	                INNER JOIN profitwisemastervariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN profitwiseproduct t3 ON t2.PwMasterProductId = t3.PwMasterProductId
                WHERE (t1.PwShopId = @PwShopId AND t2.PwShopId = @PwShopId AND t3.PwShopId = @PwShopId)";

            string term0 = "",term1 = "", term2 = "", term3 = "", term4 = "";

            if (searchTerms.Count > 0)
            {
                term0 = searchTerms[0].PrepForLike();
                query += PickListClauseHelper("term0");
            }
            if (searchTerms.Count > 1)
            {
                term1 = searchTerms[1].PrepForLike();
                query += PickListClauseHelper("term1");
            }
            if (searchTerms.Count > 2)
            {
                term2 = searchTerms[2].PrepForLike();
                query += PickListClauseHelper("term2");
            }
            if (searchTerms.Count > 3)
            {
                term3 = searchTerms[3].PrepForLike();
                query += PickListClauseHelper("term3");
            }
            if (searchTerms.Count > 4)
            {
                term4 = searchTerms[4].PrepForLike();
                query += PickListClauseHelper("term4");
            }

            _connection.Execute(
                query, new {PwShopId = this.PwShop.PwShopId, @PwPickListId = pickListId,
                            term0, term1, term2, term3, term4 });
        }

        private string PickListClauseHelper(string termName)
        {
            return $" AND ( (t1.Title LIKE @{termName}) OR (t1.Sku LIKE @{termName}) OR ( t3.Title LIKE @{termName} ) OR ( t3.Vendor LIKE @{termName} ) )";
        }

        public void FilterPickList(long pickListId, IList<ProductSearchFilter> filters)
        {
            var filterClause = "";
            var searchByVendor = "";
            var searchByProductType = "";
            var searchByTags0 = "";
            var searchByTags1 = "";
            var searchByTags2 = "";
            var searchByTags3 = "";
            var searchByTags4 = "";

            if (filters.Any(x => x.Type == ProductSearchFilterType.ProductVendor))
            {
                searchByVendor = filters.First(x => x.Type == ProductSearchFilterType.ProductVendor).Value;
                filterClause = filterClause + " AND Vendor = @searchByVendor";
            }
            if (filters.Any(x => x.Type == ProductSearchFilterType.ProductType))
            {
                searchByProductType = filters.First(x => x.Type == ProductSearchFilterType.ProductType).Value;
                filterClause = filterClause + " AND ProductType = @searchByProductType";
            }
            if (filters.Any(x => x.Type == ProductSearchFilterType.TaggedWith))
            {
                var searchByTags = filters.First(x => x.Type == ProductSearchFilterType.TaggedWith).Value;

                var tags = searchByTags.SplitBy(',');

                if (tags.Count > 0)
                {
                    searchByTags0 = tags[0].PrepForLike();
                    filterClause = filterClause + " AND Tags LIKE @searchByTags0";
                }
                if (tags.Count > 1)
                {
                    searchByTags1 = tags[1].PrepForLike();
                    filterClause = filterClause + " AND Tags LIKE @searchByTags1";
                }
                if (tags.Count > 2)
                {
                    searchByTags2 = tags[2].PrepForLike();
                    filterClause = filterClause + " AND Tags LIKE @searchByTags2";
                }
                if (tags.Count > 3)
                {
                    searchByTags3 = tags[3].PrepForLike();
                    filterClause = filterClause + " AND Tags LIKE @searchByTags3";
                }
                if (tags.Count > 4)
                {
                    searchByTags4 = tags[4].PrepForLike();
                    filterClause = filterClause + " AND Tags LIKE @searchByTags4";
                }
            }

            var query =
                @"DELETE FROM profitwisepicklistmasterproduct
                WHERE PwShopId = @PwShopId
                AND PwPickListId = @PwPickListId
                AND PwMasterProductId NOT IN (
                    SELECT PwMasterProductId
                    FROM profitwiseproduct
                    WHERE PwShopId = @PwShopId " + filterClause + ");";

            _connection.Execute(query, new
            {
                PwShopId = this.PwShop.PwShopId, @PwPickListId = pickListId, searchByVendor, searchByProductType, 
                searchByTags0, searchByTags1, searchByTags2, searchByTags3, searchByTags4,
            });
        }

        public void FilterPickListMissingCogs(long pickListId)
        {
            var query =
                @"DELETE FROM profitwisepicklistmasterproduct
                WHERE PwShopId = @PwShopId
                AND PwPickListId = @PwPickListId
                AND PwMasterProductId NOT IN (
                    SELECT DISTINCT(PwMasterProductId)
                    FROM profitwisemastervariant
                    WHERE PwShopId = @PwShopId
                    AND CogsAmount IS NULL );";

            _connection.Execute(query, new {PwShopId = PwShop.PwShopId, PwPickListId = pickListId});
        }



        // Pick List retrieve operations
        public int RetreivePickListCount(long pickListId)
        {
            var query = @"SELECT COUNT(*) 
                        FROM profitwisepicklistmasterproduct 
                        WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId";

            return _connection.Query<int>(query, new { PwShopId = PwShop.PwShopId, pickListId }).First();
        }

        public PwCogsProduct RetrieveProduct(int masterProductId)
        {
            var query =
                @"SELECT PwMasterProductId, PwProductId, Title, Vendor
                FROM profitwiseproduct
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId = @PwMasterProductId
                AND IsPrimary = true;";

            return _connection
                    .Query<PwCogsProduct>(
                        query, new
                        {
                            PwShopId = this.PwShop.PwShopId,
                            PwMasterProductId = masterProductId,
                        })
                    .FirstOrDefault();
        }

        public IList<PwCogsProduct> 
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
	                SELECT PwMasterProductId FROM profitwisepicklistmasterproduct WHERE PwShopId = @PwShopId ) " +
                sortByClause +
                " LIMIT @StartRecord, @ResultsPerPage;";

            return _connection.Query<PwCogsProduct>(
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
        public IList<PwCogsVariant> RetrieveVariants(IList<long> masterProductIds)
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

            return _connection.Query<PwCogsVariant>(
                query, new {PwShopId = this.PwShop.PwShopId, MasterProductIds = masterProductIds}).ToList();
        }

        public void UpdateProductCogsAllVariants(long masterProductId, int currencyId, decimal amount)
        {
            var query = @"UPDATE profitwisemastervariant
                        SET CogsAmount = @amount, CogsCurrencyId = @currencyId
                        WHERE PwMasterProductId = @masterProductId";

            _connection.Execute(query, new {masterProductId, currencyId, amount});
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
    }
}
