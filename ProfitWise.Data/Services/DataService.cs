using System.Collections.Generic;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.GoodsOnHand;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;
using ServiceStack.Text;

namespace ProfitWise.Data.Services
{
    // Intended for enabiling the asynchronous scheduling of data exports
    [Intercept(typeof(ShopRequired))]
    public class DataService
    {
        private readonly MultitenantFactory _factory;
        private readonly ConnectionWrapper _connectionWrapper;

        public PwShop PwShop { get; set; }

        public DataService(MultitenantFactory factory, ConnectionWrapper connectionWrapper)
        {
            _factory = factory;
            _connectionWrapper = connectionWrapper;
        }

        public List<GroupedTotal> ProfitabilityDetails(
                long reportId, ReportGrouping grouping, Model.Profit.ColumnOrdering ordering, 
                int pageNumber, int pageSize)
        {
            var repository = _factory.MakeReportRepository(PwShop);
            var queryRepository = _factory.MakeProfitRepository(PwShop);

            using (var trans = _connectionWrapper.InitiateTransaction())
            {
                var report = repository.RetrieveReport(reportId);

                queryRepository.PopulateQueryStub(reportId);
                var queryContext = new TotalQueryContext(PwShop)
                {
                    PwReportId = reportId,
                    StartDate = report.StartDate,
                    EndDate = report.EndDate,
                    Grouping = grouping,
                    Ordering = ordering,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };

                var totals = queryRepository.RetrieveTotalsByContext(queryContext);

                var executiveSummary = queryRepository.RetreiveTotalsForAll(queryContext);
                var allProfit = executiveSummary.TotalProfit;

                // Compute Profit % for each line item
                foreach (var groupTotal in totals)
                {
                    groupTotal.ProfitPercentage =
                        allProfit == 0 ? 0m : (groupTotal.TotalProfit / allProfit) * 100m;
                }

                trans.Commit();

                return totals;
            }
        }

        public List<Details> GoodsOnHandDetails(
                long reportId, Model.GoodsOnHand.ColumnOrdering ordering, int pageNumber, int pageSize,
                ReportGrouping? grouping = null, string productType = null, string vendor = null, 
                long? pwProductId = null)
        {
            var repository = _factory.MakeGoodsOnHandRepository(PwShop);
            var details = repository.RetrieveDetails(
                reportId, grouping.Value, ordering, pageNumber, pageSize,
                productType, vendor, pwProductId);

            return details;
        }
    }
}

