using System;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.GoodsOnHand;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Web.Attributes;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Json;


namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class GoodsOnHandServiceController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly IPushLogger _logger;

        public const int NumberOfColumnGroups = 5;
        public const string AllOtherGroupingName = "All other";
        public const string NoGroupingName = "All"; // Date-bucketed Totals


        public GoodsOnHandServiceController(MultitenantFactory factory, IPushLogger logger)
        {
            _factory = factory;
            _logger = logger;
        }


        [HttpPost]
        public ActionResult Data(
                long reportId, ColumnOrdering ordering = ColumnOrdering.PotentialRevenueDescending,
                string productType = null, string vendor = null, long? pwProductId = null,
                int pageNumber = 1, int pageSize = 50)
        {
            var userIdentity = HttpContext.PullIdentity();
            var queryRepository = _factory.MakeGoodsOnHandRepository(userIdentity.PwShop);
            var reportRepository = _factory.MakeReportRepository(userIdentity.PwShop);

            using (var trans = new TransactionScope())
            {
                // First create the query stub...
                queryRepository.PopulateQueryStub(reportId);
                var report = reportRepository.RetrieveReport(reportId);
                
                // Next build the top-performing summary
                var totals = queryRepository.RetrieveTotals(reportId);
                var details = queryRepository.RetrieveDetails(
                    reportId, report.GroupingId, ordering, pageNumber, pageSize);

                var chartData =
                    new object[]
                    {
                        new {
                            name = "Cost of Goods on Hand",
                            data = details.Select(detail => new HighChartElement
                            {
                                y = detail.TotalCostOfGoodsSold,
                                name = detail.GroupingName,
                                drilldown = false,
                                drilldownurl = null,
                            }).ToList()
                        }
                    };

                var detailsCount = queryRepository.DetailsCount(reportId, report.GroupingId);

                trans.Complete();
                
                return new JsonNetResult(
                    new {   userIdentity.PwShop.CurrencyId, Totals = totals, Details = details,
                            Chart = chartData, DetailsCount = detailsCount, });
            }
        }

        
        private string DrilldownUrlBuilder(
                long reportId, ColumnOrdering ordering, string productType = null, string vendor = null, long? pwProductId = null,
                int pageNumber = 1, int pageSize = 50)
        {
            return $"/GoodsOnHandService/Data?reportId={reportId}&ordering={ordering}&" +
                   $"pageNumber={pageNumber}&pageSize={pageSize}";
        }
        
    }
}

