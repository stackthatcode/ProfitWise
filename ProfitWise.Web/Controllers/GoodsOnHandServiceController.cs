using System;
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
                var details = queryRepository.RetrieveDetails(reportId, report.GroupingId, ordering, pageNumber, pageSize);
                var detailsCount = queryRepository.DetailsCount(reportId, report.GroupingId);

                trans.Complete();
                
                return new JsonNetResult(
                    new {   userIdentity.PwShop.CurrencyId, Totals = totals, Details = details,
                            DetailsCount = detailsCount, });
            }
        }
        

        
        private string DrilldownUrlBuilder(
                long reportId, ReportGrouping grouping, string key, string name, DateTime start, DateTime end)
        {
            return $"/ProfitService/Drilldown?reportId={reportId}&grouping={grouping}&key={key}&name={name}&" +
                   $"start={HttpUtility.UrlEncode(start.ToString("yyyy-MM-dd"))}&" +
                   $"end={HttpUtility.UrlEncode(end.ToString("yyyy-MM-dd"))}";
        }
        
    }
}

