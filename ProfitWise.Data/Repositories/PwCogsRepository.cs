﻿using System;
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

        public long DeletePickList()
        {
            var query =
                @"DELETE FROM profitwisequerymasterproduct WHERE PwShopId = @PwShopId;";

            return _connection.Query<long>(
                query, new {PwShopId = this.PwShop.PwShopId}).FirstOrDefault();
        }


        private string PickListClauseHelper(string termName)
        {
            return $" AND ( (t1.Title LIKE @{termName}) OR (t1.Sku LIKE @{termName}) OR ( t3.Title LIKE @{termName} ) OR ( t3.Vendor LIKE @{termName} ) )";
        }

        public void InsertPickList(List<string> terms)
        {
            var query =
                @"INSERT INTO profitwisequerymasterproduct (PwShopId, PwMasterProductId)
                SELECT DISTINCT @PwShopId, t2.PwMasterProductId
                FROM profitwisevariant t1 
	                INNER JOIN profitwisemastervariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN profitwiseproduct t3 ON t2.PwMasterProductId = t3.PwMasterProductId
                WHERE (t1.PwShopId = @PwShopId AND t2.PwShopId = @PwShopId AND t3.PwShopId = @PwShopId)";

            string term0 = "",term1 = "", term2 = "", term3 = "", term4 = "";

            if (terms.Count > 0)
            {
                term0 = terms[0].PrepForLike();
                query += PickListClauseHelper("term0");
            }
            if (terms.Count > 1)
            {
                term1 = terms[1].PrepForLike();
                query += PickListClauseHelper("term1");
            }
            if (terms.Count > 2)
            {
                term2 = terms[2].PrepForLike();
                query += PickListClauseHelper("term2");
            }
            if (terms.Count > 3)
            {
                term3 = terms[3].PrepForLike();
                query += PickListClauseHelper("term3");
            }
            if (terms.Count > 4)
            {
                term4 = terms[4].PrepForLike();
                query += PickListClauseHelper("term4");
            }

            _connection.Execute(
                query, new {PwShopId = this.PwShop.PwShopId, term0, term1, term2, term3, term4 });
        }

        public void FilterPickList(IList<ProductSearchFilter> filters)
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
                @"DELETE FROM profitwisequerymasterproduct
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId NOT IN (
                    SELECT PwMasterProductId
                    FROM profitwiseproduct
                    WHERE PwShopId = @PwShopId " + filterClause + ");";

            _connection.Execute(query, new
            {
                PwShopId = this.PwShop.PwShopId, searchByVendor, searchByProductType, 
                    searchByTags0, searchByTags1, searchByTags2, searchByTags3, searchByTags4,
            });
        }

        public void FilterPickListMissingCogs()
        {
            var query =
                @"DELETE FROM profitwisequerymasterproduct
                WHERE PwShopId = @PwShopId
                AND PwMasterProductId NOT IN (
                    SELECT DISTINCT(PwMasterProductId)
                    FROM profitwisemastervariant
                    WHERE PwShopId = @PwShopId
                    AND CogsAmount IS NULL
                );";

            _connection.Execute(query, new {PwShopId = PwShop.PwShopId});
        }

        public int RetreivePickListCount()
        {
            var query = @"SELECT COUNT(*) 
                        FROM profitwisequerymasterproduct 
                        WHERE PwShopId = @PwShopId";

            return _connection.Query<int>(query, new { PwShopId = PwShop.PwShopId }).First();
        }

        public PwCogsProductSearchResult RetrieveMasterProduct(int masterProductId)
        {
            var query =
                @"SELECT t1.PwMasterProductId, t1.PwProductId, t1.Title, t1.Vendor
                FROM profitwiseproduct
                WHERE t1.PwShopId = @PwShopId AND t1.IsPrimary = true;";

            return _connection
                    .Query<PwCogsProductSearchResult>(
                        query, new { PwShopId = this.PwShop.PwShopId })
                    .FirstOrDefault();
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
