using System.Collections.Generic;
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
        public const int FixedPageSize = 10;

        public GoodsOnHandServiceController(MultitenantFactory factory, IPushLogger logger)
        {
            _factory = factory;
            _logger = logger;
        }


        [HttpPost]
        public ActionResult Data(
                long reportId, ColumnOrdering ordering = ColumnOrdering.PotentialRevenueDescending,
                string productType = null, string vendor = null, long? pwProductId = null,
                int pageNumber = 1)
        {
            var userIdentity = HttpContext.PullIdentity();
            var queryRepository = _factory.MakeGoodsOnHandRepository(userIdentity.PwShop);
            var reportRepository = _factory.MakeReportRepository(userIdentity.PwShop);

            using (var trans = new TransactionScope())
            {
                queryRepository.PopulateQueryStub(reportId);
                var report = reportRepository.RetrieveReport(reportId);

                var results = BuildResults(
                    reportId, report.GroupingId, ordering, productType, vendor, pwProductId, pageNumber);

                return new JsonNetResult(results);
            }
        }

        public ActionResult DrillDown(
                    long reportId, ColumnOrdering ordering, ReportGrouping grouping,
                    string productType = null, string vendor = null, long? pwProductId = null,
                    int pageNumber = 1)
        {
            var results = BuildResults(
                reportId, grouping, ordering, productType, vendor, pwProductId, pageNumber);
            return new JsonNetResult(results);
        }

        private Results BuildResults(
                long reportId, ReportGrouping grouping, ColumnOrdering ordering,
                string productType = null, string vendor = null, long? pwProductId = null,
                int pageNumber = 1)
        {
            var userIdentity = HttpContext.PullIdentity();
            var queryRepository = _factory.MakeGoodsOnHandRepository(userIdentity.PwShop);

            var totals = queryRepository.RetrieveTotals(reportId);
            var detailsCount = 
                    queryRepository.DetailsCount(
                        reportId, grouping, productType, vendor, pwProductId);

            var details = queryRepository.RetrieveDetails(
                    reportId, grouping, ordering, pageNumber, FixedPageSize,
                    productType, vendor, pwProductId);

            var chartElements = new List<HighChartElement>();

            foreach (var detail in details)
            {
                var drillDownUrl = DrilldownUrlBuilder(
                    reportId, grouping, detail, productType, vendor, pwProductId);

                chartElements.Add(
                    new HighChartElement
                    {
                        grouping = grouping,
                        y = detail.TotalCostOfGoodsSold,
                        name = detail.GroupingName,
                        drilldown = drillDownUrl != null,
                        drilldownurl = drillDownUrl,
                    });
            }

            var chart = new List<ChartData>
                {
                    new ChartData()
                    {
                        name = "Cost of Goods on Hand",
                        data = chartElements,
                        showInLegend = false,
                    }
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
                long reportId, ReportGrouping currentGrouping, Details currentDetails,                
                string productType = null, string vendor = null, long? pwProductId = null)
        {
            if (currentGrouping == ReportGrouping.Variant)
            {
                return null;
            }

            var url = $"/GoodsOnHandService/DrillDown?reportId={reportId}";

            // First add the current drill down state, as reflected in the URL
            if (productType != null)
            {
                url += $"&productType={HttpUtility.UrlEncode(productType)}";
            }
            if (vendor != null)
            {
                url += $"&vendor={HttpUtility.UrlEncode(vendor)}";
            }
            if (pwProductId != null)
            {
                url += $"&pwProductId={HttpUtility.UrlEncode(pwProductId.ToString())}";
            }

            // Next, for this Details record, add a drill down pathway
            var encodedDrilldownKey = HttpUtility.UrlEncode(currentDetails.GroupingKey);

            if (currentGrouping == ReportGrouping.ProductType)
            {
                // If this is the current grouping, Product Type won't have been added
                url += $"&productType={encodedDrilldownKey}&grouping={ReportGrouping.Vendor}";
                return url;
            }
            if (currentGrouping == ReportGrouping.Vendor)
            {
                // If this is the current grouping, Vendor won't have been added
                url += $"&vendor={encodedDrilldownKey}&grouping={ReportGrouping.Product}";
                return url;
            }
            if (currentGrouping == ReportGrouping.Product)
            {
                // If this is the current grouping, Product won't have been added
                url += $"&pwProductId={encodedDrilldownKey}&grouping={ReportGrouping.Variant}";
                return url;
            }

            return url;
        }
     
    }
}

