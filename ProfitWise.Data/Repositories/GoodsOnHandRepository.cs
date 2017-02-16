using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;


namespace ProfitWise.Data.Repositories
{
    public class GoodsOnHandRepository
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;
        private readonly MultitenantFactory _factory;


        public GoodsOnHandRepository(
                ConnectionWrapper connectionWrapper, MultitenantFactory factory)
        {
            _connectionWrapper = connectionWrapper;
            _factory = factory;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }

        public void PopulateQueryStub(long reportId)
        {
            var filterRepository = _factory.MakeReportFilterRepository(this.PwShop);

            var deleteQuery =
                @"DELETE FROM profitwisegoodsonhandquerystub
                WHERE PwShopId = @PwShopId AND PwReportId = @PwReportId";
            Connection.Execute(deleteQuery, new { PwShopId, PwReportId = reportId });

            var createQuery =
                @"INSERT INTO profitwisegoodsonhandquerystub
                SELECT @PwReportId, @PwShopId, PwVariantId, PwProductId, 
                        Vendor, ProductType, ProductTitle, Sku, VariantTitle
                FROM vw_standaloneproductandvariantsearch 
                WHERE PwShopId = @PwShopId " +
                filterRepository.ReportFilterClauseGenerator(reportId) +
                @" GROUP BY PwVariantId, PwProductId, 
                Vendor, ProductType, ProductTitle, Sku, VariantTitle; ";
            Connection.Execute(createQuery, new { PwShopId, PwReportId = reportId });
        }


        public List<ReportSelectionMasterProduct> RetrieveTotals(long reportId)
        {
            throw new NotImplementedException();
        }
    }
}

