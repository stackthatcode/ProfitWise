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
using ServiceStack.Text;


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
        public ActionResult Populate(long reportId)
        {
            var userIdentity = HttpContext.PullIdentity();
            var queryRepository = _factory.MakeGoodsOnHandRepository(userIdentity.PwShop);
            
            using (var trans = new TransactionScope())
            {
                queryRepository.PopulateQueryStub(reportId);
                trans.Complete();
            }
            return JsonNetResult.Success();
        }

        [HttpGet]
        public ActionResult Data(
                    long reportId, ColumnOrdering ordering, ReportGrouping? grouping = null,
                    string productType = null, string vendor = null, long? pwProductId = null,
                    int pageNumber = 1)
        {
            if (grouping == null)
            {
                var userIdentity = HttpContext.PullIdentity();
                var reportRepository = _factory.MakeReportRepository(userIdentity.PwShop);
                var report = reportRepository.RetrieveReport(reportId);
                grouping = report.GroupingId;
            }

            var results = BuildResults(
                reportId, grouping.Value, ordering, productType, vendor, pwProductId, pageNumber);
            return new JsonNetResult(results);
        }

        [HttpGet]
        public ActionResult Export(
                   long reportId, ColumnOrdering ordering, ReportGrouping? grouping = null,
                   string productType = null, string vendor = null, long? pwProductId = null,
                   int pageNumber = 1)
        {
            var userIdentity = HttpContext.PullIdentity();
            if (grouping == null)
            {
                var reportRepository = _factory.MakeReportRepository(userIdentity.PwShop);
                var report = reportRepository.RetrieveReport(reportId);
                grouping = report.GroupingId;
            }

            var service = _factory.MakeDataService(userIdentity.PwShop);
            var details = service.GoodsOnHandDetails(
                reportId, ordering, pageNumber, 100000, grouping, productType, vendor, pwProductId);

            string csv = CsvSerializer.SerializeToCsv(details);
            return File(new System.Text.UTF8Encoding().GetBytes(csv), "text/csv", "GoodsOnHandDetail.csv");
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
                var drillDownQueryString = 
                    DrilldownQueryStringBuilder(grouping, detail, productType, vendor, pwProductId);

                chartElements.Add(
                    new HighChartElement
                    {
                        grouping = grouping,
                        y = detail.TotalCostOfGoodsSold,
                        name = detail.GroupingName,
                        drilldown = drillDownQueryString != null,
                        querystringbase = drillDownQueryString,
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

        private string DrilldownQueryStringBuilder(
                ReportGrouping currentGrouping, Details currentDetails, string productType = null, 
                string vendor = null, long? pwProductId = null)
        {
            if (currentGrouping == ReportGrouping.Variant)
            {
                return null;
            }

            var url = "";

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

