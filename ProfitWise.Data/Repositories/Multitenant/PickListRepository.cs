using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Repositories.Multitenant
{
    public class PickListRepository
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;

        public PickListRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }



        public long CreateNew()
        {
            var query =
                @"INSERT INTO picklist(@PwShopId) (PwShopId, CreatedDate, LastAccessed) 
                    VALUES (@PwShopId, @createdDate, @createdDate);
                SELECT SCOPE_IDENTITY();";

            return _connectionWrapper.Query<long>(query,
                    new { PwShop.PwShopId, createdDate = DateTime.Now }).First();
        }        

        public void Delete(long pickListId)
        {
            var query =
                @"DELETE FROM picklistmasterproduct(@PwShopId) WHERE PwPickListId = @pickListId;
                DELETE FROM picklist(@PwShopId) WHERE PwPickListId = @pickListId;";
            _connectionWrapper.Execute(query, new { PwShop.PwShopId, pickListId});
        }

        // Pick List population/filter operations
        public void Populate(long pickListId, List<string> searchTerms)
        {
            var query =
                @"INSERT INTO picklistmasterproduct(@PwShopId) (PwShopId, PwPickListId, PwMasterProductId)
                SELECT DISTINCT @PwShopId, @PwPickListId, t2.PwMasterProductId
                FROM variant(@PwShopId) t1 
	                INNER JOIN mastervariant(@PwShopId) t2 ON t1.PwMasterVariantId = t2.PwMasterVariantId
	                INNER JOIN product(@PwShopId) t3 ON t2.PwMasterProductId = t3.PwMasterProductId ";

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

            _connectionWrapper.Execute(
                query, new { PwShop.PwShopId, @PwPickListId = pickListId, term0, term1, term2, term3, term4});
        }

        private string PickListClauseHelper(string termName)
        {
            return $" AND ( (t1.Title LIKE @{termName}) OR (t1.Sku LIKE @{termName}) OR ( t3.Title LIKE @{termName} ) OR ( t3.Vendor LIKE @{termName} ) )";
        }

        public void Filter(long pickListId, long pwMasterProductId)
        {
            var query = @"DELETE FROM picklistmasterproduct(@PwShopId)
                        WHERE PwPickListId = @PwPickListId AND PwMasterProductId = @PwMasterProductId";
            _connectionWrapper.Execute(query, 
                new {PwShop.PwShopId, PwPickListId = pickListId, PwMasterProductId = pwMasterProductId});
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
                @"DELETE FROM picklistmasterproduct(@PwShopId)
                WHERE PwPickListId = @PwPickListId
                AND PwMasterProductId NOT IN (
                    SELECT PwMasterProductId
                    FROM product(@PwShopId)
                    WHERE PwShopId = PwShopId " + filterClause + ");";

            _connectionWrapper.Execute(query, new
            {
                PwShop.PwShopId,
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
                @"DELETE FROM picklistmasterproduct(@PwShopId)
                WHERE PwPickListId = @PwPickListId
                AND PwMasterProductId NOT IN (
                    SELECT DISTINCT(PwMasterProductId)
                    FROM mastervariant(@PwShopId)
                    WHERE ((CogsTypeId = 1 AND (CogsAmount IS NULL OR CogsAmount = 0)) 
                    OR (CogsTypeId = 2 AND (CogsMarginPercent IS NULL OR CogsMarginPercent = 0))));";

            _connectionWrapper.Execute(query, new { PwShop.PwShopId, PwPickListId = pickListId });
        }
        

        // Pick List retrieve operations
        public int Count(long pickListId)
        {
            var query = @"SELECT COUNT(*) FROM picklistmasterproduct(@PwShopId) WHERE PwPickListId = @pickListId";

            return _connectionWrapper.Query<int>(query, new { PwShop.PwShopId, pickListId }).First();
        }


        public bool Exists(long pickListId)
        {
            var query = @"SELECT * FROM picklist(@PwShopId) WHERE PwPickListId = @pickListId";
            return _connectionWrapper.Query<object>(query, new { PwShop.PwShopId, pickListId }).Any();
        }
    }
}
