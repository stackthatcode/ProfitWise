using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
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

        public class ColumnChartData
        {
            public List<ReportSeries> Series { get; set; }
            public List<ReportSeries> Drilldown { get; set; }
        }

        [HttpPost]
        public ActionResult Dataset(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var queryRepository = _factory.MakeReportQueryRepository(userBrief.PwShop);


            using (var transaction = repository.InitiateTransaction())
            {
                var shopCurrencyId = userBrief.PwShop.CurrencyId;
                var report = repository.RetrieveReport(reportId);

                // First create the query stub...
                queryRepository.PopulateQueryStub(reportId);

                // Next build the top-performing summary
                var summary = BuildSummary(report, userBrief.PwShop);
                ColumnChartData columnChartData;
                
                var dateTotals =
                    queryRepository
                        .RetrieveOverallDateTotals(report.PwReportId, report.StartDate, report.EndDate)
                        .ToDictionary(x => x.OrderDate, x => x);

                if (report.GroupingId == ReportGrouping.Overall)
                {
                    var granularity = (report.EndDate - report.StartDate).ToDefaultGranularity();
                    var series = BuildSeriesFromDateTotals(
                            "All", report.StartDate, report.EndDate, granularity, dateTotals);
                    
                    columnChartData = new ColumnChartData
                    {
                        Series = new List<ReportSeries> { series },
                        Drilldown = new List<ReportSeries>()
                    };
                }
                else
                {
                    columnChartData = BuildColumnChartSeriesNonOverall(userBrief.PwShop, report, summary);
                }
                
                transaction.Commit();

                return new JsonNetResult(
                    new
                    {
                        CurrencyId = shopCurrencyId,
                        Summary = summary,
                        Series = columnChartData.Series,
                        Drilldown = columnChartData.Drilldown
                    });
            }
        }


        private Summary BuildSummary(PwReport report, PwShop shop)
        {
            long reportId = report.PwReportId;
            var queryRepository = _factory.MakeReportQueryRepository(shop);

            var executiveSummary =
                queryRepository.RetreiveTotalsForAll(reportId, report.StartDate, report.EndDate);

            var productTotals =
                queryRepository.RetreiveTotalsByProduct(reportId, report.StartDate, report.EndDate)
                    .AppendAllOthersAsDifferenceOfSummary(executiveSummary);
            var variantTotals =
                queryRepository.RetreiveTotalsByVariant(reportId, report.StartDate, report.EndDate)
                    .AppendAllOthersAsDifferenceOfSummary(executiveSummary);
            var productTypeTotals =
                queryRepository.RetreiveTotalsByProductType(reportId, report.StartDate, report.EndDate)
                    .AppendAllOthersAsDifferenceOfSummary(executiveSummary);
            var vendorTotals =
                queryRepository.RetreiveTotalsByVendor(reportId, report.StartDate, report.EndDate)
                    .AppendAllOthersAsDifferenceOfSummary(executiveSummary);

            var summary = new Summary()
            {
                CurrencyId = shop.CurrencyId,
                ExecutiveSummary = executiveSummary,
                ProductsByMostProfitable = productTotals,
                VariantByMostProfitable = variantTotals,
                ProductTypeByMostProfitable = productTypeTotals,
                VendorsByMostProfitable = vendorTotals,
            };
            return summary;
        }

        private ColumnChartData BuildColumnChartSeriesNonOverall(PwShop shop, PwReport report, Summary summary)
        {
            var queryRepository = _factory.MakeReportQueryRepository(shop);
            var seriesDataset = new List<ReportSeries>();
            var drilldownDataset = new List<ReportSeries>();

            // 1st-level drill down
            var granularity = (report.EndDate - report.StartDate).ToDefaultGranularity();
            var topLevelTotals =
                queryRepository.RetrieveCanonizedDateTotals(
                    report.PwReportId, report.StartDate, report.EndDate, report.GroupingId, granularity);

            //  The Grouping Names should match with those from the Summary
            var groupingNames = summary
                .TotalsByGroupingId(report.GroupingId)
                .Take(NumberOfColumnGroups)
                .Select(x => x.GroupingName)
                .ToList();

            foreach (var groupingName in groupingNames)
            {
                var series = BuildSeriesFromCanonizedDateTotals(
                    groupingName, report.StartDate, report.EndDate, granularity, topLevelTotals);
                seriesDataset.Add(series);
            }

            if (granularity != DataGranularity.Day)
            {
                var drilldownGranularity = granularity.NextDrilldownLevel();
                var drilldownTotals =
                    queryRepository.RetrieveCanonizedDateTotals(
                        report.PwReportId, report.StartDate, report.EndDate, report.GroupingId, drilldownGranularity);

                foreach (var element in seriesDataset.SelectMany(x => x.data))
                {
                    var series = BuildSeriesFromCanonizedDateTotals(
                        element.Parent.name, element.StartDate, element.EndDate, drilldownGranularity, drilldownTotals);

                    element.drilldown = element.CanonizedIdentifier;
                    series.id = element.CanonizedIdentifier;

                    drilldownDataset.Add(series);
                }
            }

            return new ColumnChartData { Series = seriesDataset, Drilldown = drilldownDataset };
        }

        private ReportSeries BuildSeriesFromCanonizedDateTotals(
                    string groupingName, DateTime start, DateTime end, DataGranularity granularity,
                    List<CanonizedDateTotal> dateBucketedTotals)
        {
            var series = ReportSeriesFactory.GenerateSeries(groupingName, start, end, granularity);
            series.id = groupingName;   // The jury is out on this one!!

            foreach (var element in series.data)
            {
                var total = dateBucketedTotals.FirstOrDefault(
                        x => x.CanonizedIdentifier == element.CanonizedIdentifier);

                if (total != null)
                {
                    element.y = total.TotalProfit;
                }
            }

            return series;
        }

        private ReportSeries BuildSeriesFromDateTotals(
                    string groupingName, DateTime start, DateTime end, 
                    DataGranularity granularity, Dictionary<DateTime, DateTotal> dateTotals)
        {
            var series = ReportSeriesFactory.GenerateSeries(groupingName, start, end, granularity);
            series.id = groupingName;   // The jury is out on this one!!

            foreach (var element in series.data)
            {
                var total = 0m;
                var current = element.StartDate;
                while (current <= element.EndDate)
                {
                    if (dateTotals.ContainsKey(current))
                    {
                        total += dateTotals[current].TotalProfit;
                    }
                    current = current.AddDays(1);
                }
                element.y = total;
            }

            return series;
        }
    }
}

