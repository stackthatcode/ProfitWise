using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class QueryServiceController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;

        public const int NumberOfColumnGroups = 5;


        public QueryServiceController(MultitenantFactory factory, CurrencyService currencyService)
        {
            _factory = factory;
            _currencyService = currencyService;
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

            // Retreive Report
            var reportRepository = _factory.MakeReportRepository(userBrief.PwShop);
            var report = reportRepository.RetrieveReport(reportId);

            var orderLineProfits = BuildProfitabilityDataset(reportId, userBrief, report);

            // Summary for consumption by pie chart
            var shopCurrencyId = userBrief.PwShop.CurrencyId;
            var summary = orderLineProfits.BuildCompleteGroupedSummary(shopCurrencyId);


            // Generate Series for Drill Down
            var granularity = (report.EndDate - report.StartDate).SuggestedDataGranularity();
            List<ReportSeries> seriesDataset;

            if (report.GroupingId == ReportGrouping.Overall)
            {
                var series = ReportSeriesFactory.GenerateSeries(report.StartDate, report.EndDate, granularity);
                series.PopulateWithTotals(orderLineProfits, x => true);
                seriesDataset = new List<ReportSeries> {series};
            }
            else
            { 
                var orderedTotalsByGroup =
                        summary
                            .TotalsByGroupedId(report.GroupingId)
                            .Take(NumberOfColumnGroups)
                            .ToList();

                seriesDataset =
                    BuildSeriesDatasetFromOrderedTotals(
                        orderLineProfits, orderedTotalsByGroup, report.StartDate, report.EndDate, granularity);
            }

            return new JsonNetResult(new { CurrencyId = shopCurrencyId, Summary = summary, Series = seriesDataset });
        }

        private static List<ReportSeries>
                    BuildSeriesDatasetFromOrderedTotals(
                        IList<OrderLineProfit> orderLines, IList<GroupedTotal> orderedTotals, 
                        DateTime startDate, DateTime endDate, DataGranularity granularity)
        {
            var output = new List<ReportSeries>();
            foreach (var groupedTotal in orderedTotals)
            {
                var series = ReportSeriesFactory.GenerateSeries(startDate, endDate, granularity);
                var groupingKey = groupedTotal.GroupingKey;
                series.PopulateWithTotals(orderLines, x => groupingKey.MatchWithOrderLine(x));
                output.Add(series);
            }
            return output;
        }

        private IList<OrderLineProfit> 
                BuildProfitabilityDataset(
                    long reportId, IdentitySnapshot userBrief, PwReport report)
        {
            var queryRepository = _factory.MakeReportQueryRepository(userBrief.PwShop);
            var shopCurrencyId = userBrief.PwShop.CurrencyId;

            // Transforms Report Filters into Master Variants to join with Order Line data
            queryRepository.PopulateQueryStub(reportId);

            // Retrieve by Date Range and assign CoGS to all the Order Lines to compute Profits
            var orderLineProfits = queryRepository.RetrieveOrderLineProfits(
                reportId, report.StartDate, report.EndDate);

            // The Search Stubs for later report output
            var searchStubs =
                queryRepository
                    .RetrieveSearchStubs(reportId)
                    .ToDictionary(x => x.PwMasterVariantId);

            // CoGS data for this Report
            var cogs =
                queryRepository
                    .RetrieveCogsData(reportId)
                    .ToDictionary(x => x.PwMasterVariantId);

            // Combines CoGS and Search Stubs with the Order Lines
            PopulateCogs(orderLineProfits, searchStubs, cogs, shopCurrencyId);
            return orderLineProfits;
        }
        

        private void PopulateCogs(
                    IList<OrderLineProfit> orderLineProfits, 
                    Dictionary<long, PwReportSearchStub> searchStubs, 
                    Dictionary<long, PwReportMasterVariantCogs> cogs, 
                    int shopCurrencyId)
        {
            foreach (var line in orderLineProfits)
            {
                line.SearchStub = searchStubs[line.PwMasterVariantId];
                var cogsEntry = cogs[line.PwMasterVariantId];

                var normalizedCogsAmount =
                    cogsEntry.HasData
                        ? _currencyService.Convert(
                            cogsEntry.CogsAmount.Value, cogsEntry.CogsCurrencyId.Value, shopCurrencyId, line.OrderDate)
                        : 0m;

                line.PerUnitCogs = normalizedCogsAmount;
            }
        }

    }
}

