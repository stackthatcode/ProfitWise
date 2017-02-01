using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Repositories
{
    public class PwPickListRepository
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;

        public PwPickListRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }



        public long CreateNew()
        {
            var query =
                @"INSERT INTO profitwisepicklist (PwShopId, CreatedDate, LastAccessed) 
                    VALUES (@PwShopId, @createdDate, @createdDate);
                SELECT SCOPE_IDENTITY();";

            return Connection.Query<long>(query,
                    new { PwShopId = this.PwShop.PwShopId, createdDate = DateTime.Now }).First();
        }

        public long UnprovisionById(long pickListId)
        {
            var query =
                @"DELETE FROM profitwisepicklistmasterproduct 
                WHERE PwShopId = PwShopId AND PwPickListId = @pickListId;
                
                DELETE FROM profitwisepicklist
                WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId;";

            return Connection.Query<long>(
                query, new { PwShopId = this.PwShop.PwShopId, pickListId }).FirstOrDefault();
        }

        public void Delete(long pickListId)
        {
            var query =
                @"DELETE FROM profitwisepicklistmasterproduct 
                WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId;
                DELETE FROM profitwisepicklist 
                WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId;";

            Connection.Execute(query, new {PwShopId = this.PwShop.PwShopId, pickListId});
        }


        // Pick List population/filter operations
        public void Populate(long pickListId, List<string> searchTerms)
        {
            var query =
                @"INSERT INTO profitwisepicklistmasterproduct (PwShopId, PwPickListId, PwMasterProductId)
                SELECT DISTINCT @PwShopId, @PwPickListId, t2.PwMasterProductId
                FROM profitwisevariant t1 
	                INNER JOIN profitwisemastervariant t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN profitwiseproduct t3 ON t2.PwMasterProductId = t3.PwMasterProductId
                WHERE (t1.PwShopId = @PwShopId AND t2.PwShopId = @PwShopId AND t3.PwShopId = @PwShopId)";

            string term0 = "", term1 = "", term2 = "", term3 = "", term4 = "";

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

            Connection.Execute(
                query,
                new { PwShopId = this.PwShop.PwShopId, @PwPickListId = pickListId, term0, term1, term2, term3, term4 });
        }

        private string PickListClauseHelper(string termName)
        {
            return $" AND ( (t1.Title LIKE @{termName}) OR (t1.Sku LIKE @{termName}) OR ( t3.Title LIKE @{termName} ) OR ( t3.Vendor LIKE @{termName} ) )";
        }

        public void Filter(long pickListId, IList<ProductSearchFilter> filters)
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

            Connection.Execute(query, new
            {
                PwShopId = this.PwShop.PwShopId,
                @PwPickListId = pickListId,
                searchByVendor,
                searchByProductType,
                searchByTags0,
                searchByTags1,
                searchByTags2,
                searchByTags3,
                searchByTags4,
            });
        }

        public void FilterMissingCogs(long pickListId)
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

            Connection.Execute(query, new { PwShopId = PwShop.PwShopId, PwPickListId = pickListId });
        }
        

        // Pick List retrieve operations
        public int Count(long pickListId)
        {
            var query = @"SELECT COUNT(*) 
                        FROM profitwisepicklistmasterproduct 
                        WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId";

            return Connection.Query<int>(query, new { PwShopId = PwShop.PwShopId, pickListId }).First();
        }


        public bool Exists(long pickListId)
        {
            var query = @"SELECT * FROM profitwisepicklist
                        WHERE PwShopId = @PwShopId AND PwPickListId = @pickListId";

            return Connection.Query<object>(
                query, new { PwShopId = PwShop.PwShopId, pickListId }).Any();
        }

    }
}
