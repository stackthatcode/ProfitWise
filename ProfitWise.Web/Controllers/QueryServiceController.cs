using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class QueryServiceController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;
        private readonly IPushLogger _logger;

        public const int NumberOfColumnGroups = 5;


        public QueryServiceController(
                MultitenantFactory factory, 
                CurrencyService currencyService,
                IPushLogger logger)
        {
            _factory = factory;
            _currencyService = currencyService;
            _logger = logger;
        }


        [HttpGet]
        public ActionResult RecordCounts(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportQueryRepository(userBrief.PwShop);
            var output = repository.RetrieveReportRecordCount(reportId);
            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult ProductSelectionsByPage(long reportId, int pageNumber = 1, int pageSize = PreviewSelectionLimit.MaximumNumberOfProducts)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportQueryRepository(userBrief.PwShop);

            var limit = PreviewSelectionLimit.MaximumNumberOfProducts;
            var selections = repository.RetrieveProductSelections(reportId, pageNumber, pageSize);
            var counts = repository.RetrieveReportRecordCount(reportId);

            return new JsonNetResult(new { Selections = selections, RecordCounts = counts });
        }

        [HttpGet]
        public ActionResult VariantSelectionsByPage(long reportId, int pageNumber = 1, int pageSize = PreviewSelectionLimit.MaximumNumberOfProducts)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportQueryRepository(userBrief.PwShop);

            var limit = PreviewSelectionLimit.MaximumNumberOfVariants;
            var selections = repository.RetrieveVariantSelections(reportId, pageNumber, pageSize);
            var counts = repository.RetrieveReportRecordCount(reportId);

            return new JsonNetResult(new { Selections = selections, RecordCounts = counts });
        }


        [HttpPost]
        public ActionResult Dataset(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var reportRepository = _factory.MakeReportRepository(userBrief.PwShop);
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            // Retreive Report
            var report = reportRepository.RetrieveReport(reportId);

            using (var transaction = repository.InitiateTransaction())
            {
                var orderLineProfits = BuildProfitabilityDataset(reportId, userBrief, report.StartDate, report.EndDate);

                // Summary for consumption by pie chart
                var shopCurrencyId = userBrief.PwShop.CurrencyId;
                var summary = orderLineProfits.BuildCompleteGroupedSummary(shopCurrencyId);

                // Generate Series for Drill Down
                var granularity = (report.EndDate - report.StartDate).SuggestedDataGranularity();
                List<ReportSeries> seriesDataset;

                if (report.GroupingId == ReportGrouping.Overall)
                {
                    var series = 
                        ReportSeriesFactory.GenerateSeries(
                            "All", GroupingKey.Factory(ReportGrouping.Overall, null), report.StartDate, report.EndDate, granularity);

                    series.Populate(orderLineProfits, x => true);
                    seriesDataset = new List<ReportSeries> {series};
                }
                else
                {
                    var orderedTotalsBySelectedGroup =
                        summary
                            .TotalsByGroupedId(report.GroupingId)
                            .Take(NumberOfColumnGroups)
                            .ToList();

                    seriesDataset =
                        BuildSeriesDatasetFromOrderedTotals(
                            orderLineProfits, orderedTotalsBySelectedGroup, report, granularity);
                }

                var drilldown = new List<ReportSeries>();
                foreach (var series in seriesDataset)
                {
                    drilldown.AddRange(PopulateDrillDown(series, orderLineProfits));
                }

                transaction.Commit();

                return new JsonNetResult(
                    new {CurrencyId = shopCurrencyId, Summary = summary, Series = seriesDataset, Drilldown = drilldown});
            }
        }

        private List<ReportSeries> 
                    PopulateDrillDown(ReportSeries series, List<OrderLineProfit> orderLines)
        {
            var output = new List<ReportSeries>();

            foreach (var element in series.data)
            {
                var granularity = (element.End - element.Start).SuggestedDataGranularity();

                var drilldownSeries =
                    ReportSeriesFactory.GenerateSeries(
                        series.name,    // Series will always have the same name
                        series.GroupingKey,
                        element.Start,
                        element.End,
                        granularity);

                // *** Important - this identifier is what glues the drilldown together
                var identifier = series.name + " - " + element.name;
                drilldownSeries.id = identifier;
                element.drilldown = identifier;

                drilldownSeries.Populate(orderLines, 
                        x => series.GroupingKey.MatchWithOrderLine(x) && 
                            x.OrderDate >= element.Start &&
                            x.OrderDate <= element.End.AddDays(1).AddSeconds(-1));

                output.Add(drilldownSeries);
            }

            return output;
        }


        private void PopulateCogs(
                    IList<OrderLineProfit> orderLineProfits,
                    Dictionary<long, PwReportMasterVariantCogs> cogs,
                    int shopCurrencyId)
        {
            foreach (var line in orderLineProfits)
            {
                var cogsEntry = cogs[line.PwMasterVariantId];

                var normalizedCogsAmount =
                    cogsEntry.HasData
                        ? _currencyService.Convert(
                            cogsEntry.CogsAmount.Value, cogsEntry.CogsCurrencyId.Value, shopCurrencyId, line.OrderDate)
                        : 0m;

                line.PerUnitCogs = normalizedCogsAmount;
            }
        }

        private List<OrderLineProfit> BuildProfitabilityDataset(
                    long reportId, IdentitySnapshot userBrief, DateTime startDate, DateTime endDate)
        {
            var queryRepository = _factory.MakeReportQueryRepository(userBrief.PwShop);
            var shopCurrencyId = userBrief.PwShop.CurrencyId;

            // Transforms Report Filters into Master Variants to join with Order Line data
            queryRepository.PopulateQueryStub(reportId);

            // Retrieve by Date Range and assign CoGS to all the Order Lines to compute Profits
            var orderLineProfits = queryRepository
                    .RetrieveOrderLineProfits(reportId, startDate, endDate);
            var searchStubs = queryRepository
                    .RetrieveSearchStubs(reportId)
                    .ToDictionary(x => x.PwMasterVariantId);
            orderLineProfits.ForEach(x => x.SearchStub = searchStubs[x.PwMasterVariantId]);

            // CoGS data for this Report
            var cogs = queryRepository
                    .RetrieveCogsData(reportId)
                    .ToDictionary(x => x.PwMasterVariantId);

            // Combines CoGS and Search Stubs with the Order Lines
            PopulateCogs(orderLineProfits, cogs, shopCurrencyId);

            return orderLineProfits;
        }

        private List<ReportSeries>
                    BuildSeriesDatasetFromOrderedTotals(
                        IList<OrderLineProfit> orderLines,
                        IList<GroupedTotal> orderedTotals,
                        PwReport report,
                        DataGranularity granularity)
        {
            var output = new List<ReportSeries>();
            foreach (var groupedTotal in orderedTotals)
            {
                var series =
                    ReportSeriesFactory.GenerateSeries(
                        groupedTotal.GroupingName,  // Ultimaker 2, 3D Printers, 3DU285PLARED, etc.
                        groupedTotal.GroupingKey,
                        report.StartDate,
                        report.EndDate,
                        granularity);

                series.id = groupedTotal.GroupingName;
                var groupingKey = groupedTotal.GroupingKey;
                series.Populate(orderLines, x => groupingKey.MatchWithOrderLine(x));
                output.Add(series);
            }

            return output;
        }
    }
}

