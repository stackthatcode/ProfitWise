using System;
using System.Collections.Generic;
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
                queryRepository.PopulateQueryStub(reportId);
                var report = reportRepository.RetrieveReport(reportId);

                var results = BuildResults(
                        reportId, report.GroupingId, ordering, productType, vendor, pwProductId,
                        pageNumber, pageSize);

                return new JsonNetResult(results);
            }
        }

        public ActionResult DrillDown(
                    long reportId, ColumnOrdering ordering, ReportGrouping grouping,
                    string productType = null, string vendor = null, long? pwProductId = null,
                    int pageNumber = 1, int pageSize = 50)
        {
            var results = BuildResults(
                reportId, grouping, ordering, productType, vendor, pwProductId, pageNumber, pageSize);
            return new JsonNetResult(results);
        }

        private Results BuildResults(
                long reportId, ReportGrouping grouping, ColumnOrdering ordering,
                string productType = null, string vendor = null, long? pwProductId = null,
                int pageNumber = 1, int pageSize = 50)
        {
            var userIdentity = HttpContext.PullIdentity();
            var queryRepository = _factory.MakeGoodsOnHandRepository(userIdentity.PwShop);

            var totals = queryRepository.RetrieveTotals(reportId);
            var detailsCount = queryRepository.DetailsCount(reportId, grouping);
            var details = queryRepository.RetrieveDetails(
                    reportId, grouping, ordering, pageNumber, pageSize,
                    productType, vendor, pwProductId);

            var chartElements = new List<HighChartElement>();
            foreach (var detail in details)
            {
                var drillDownUrl = DrilldownUrlBuilder(
                    reportId, ordering, pageSize, grouping, detail, productType, vendor, pwProductId);

                chartElements.Add(new HighChartElement
                {
                    y = detail.TotalCostOfGoodsSold,
                    name = detail.GroupingName,
                    drilldown = drillDownUrl != null,
                    drilldownurl = drillDownUrl,
                });
            }

            var chart = new List<ChartData>
                {
                    new ChartData() { name = "Cost of Goods on Hand", data = chartElements }
                };

            return new Results()
            {
                CurrencyId = userIdentity.PwShop.CurrencyId,
                Chart = chart,
                Totals = totals,
                Details = details,
                DetailsCount = detailsCount,
            };
        }
        
        private string DrilldownUrlBuilder(
                long reportId, ColumnOrdering ordering, int pageSize,
                ReportGrouping currentGrouping, Details currentDetails,                
                string productType = null, string vendor = null, long? pwProductId = null)
        {
            if (currentGrouping == ReportGrouping.Variant)
            {
                return null;
            }

            var url = 
                $"/GoodsOnHandService/DrillDown?reportId={reportId}&ordering={ordering}" +
                $"&pageNumber=1&pageSize={pageSize}";

            // These are exclusive conditions, although still guarded by an "if"
            if (productType != null)
            {
                url += $"&productType={productType}";
            }
            else if (currentGrouping == ReportGrouping.ProductType)
            {
                url += $"&productType={currentDetails.GroupingKey}";
            }

            if (vendor != null)
            {
                url += $"&vendor={vendor}";
            }
            else if (currentGrouping == ReportGrouping.Vendor)
            {
                url += $"&vendor={currentDetails.GroupingKey}";
            }

            if (pwProductId != null)
            {
                url += $"&pwProductId={pwProductId}";
            }
            else if (currentGrouping == ReportGrouping.Product)
            {
                url += $"&pwProductId={currentDetails.GroupingKey}";
            }

            return url;
        }
     
    }
}

