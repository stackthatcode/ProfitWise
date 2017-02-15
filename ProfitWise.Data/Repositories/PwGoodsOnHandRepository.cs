using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Shop;


namespace ProfitWise.Data.Repositories
{
    public class PwGoodsOnHandRepository
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;
        private readonly MultitenantFactory _factory;


        public PwGoodsOnHandRepository(
                ConnectionWrapper connectionWrapper, MultitenantFactory factory)
        {
            _connectionWrapper = connectionWrapper;
            _factory = factory;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }


        // Product & Variant counts and selection details
        public ProductAndVariantCount RetrieveReportRecordCount(long reportId)
        {
            var query =
                @"SELECT COUNT(DISTINCT(PwMasterProductId)) AS ProductCount, 
                        COUNT(DISTINCT(PwMasterVariantId)) AS VariantCount
                FROM vw_MasterProductAndVariantSearch 
                WHERE PwShopId = @PwShopId ";


            return Connection
                .Query<ProductAndVariantCount>(query, new { PwShopId, PwReportId = reportId })
                .FirstOrDefault();
        }

        public List<PwReportSelectionMasterProduct> RetrieveProductSelections(long reportId, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}

