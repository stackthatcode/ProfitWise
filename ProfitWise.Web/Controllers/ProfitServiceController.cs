using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Web.Attributes;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Json;
using ServiceStack.Text;


namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [MaintenanceAttribute]
    [IdentityProcessor]
    public class ProfitServiceController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly IPushLogger _logger;

        public const int NumberOfColumnGroups = 5;
        public const string AllOtherGroupingName = "All other";
        public const string NoGroupingName = "All"; // Date-bucketed Totals


        public ProfitServiceController(MultitenantFactory factory, IPushLogger logger)
        {
            _factory = factory;
            _logger = logger;
        }


        [HttpPost]
        public ActionResult Summary(long reportId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var reportRepository = _factory.MakeReportRepository(userIdentity.PwShop);            
            var queryRepository = _factory.MakeProfitRepository(userIdentity.PwShop);
            
            using (var trans = new TransactionScope())
            {
                var shopCurrencyId = userIdentity.PwShop.CurrencyId;
                var report = reportRepository.RetrieveReport(reportId);

                // First create the query stub...
                queryRepository.PopulateQueryStub(reportId);

                // Next build the top-performing summary
                var summary = BuildSummary(report, userIdentity.PwShop);

                List<ReportSeries> seriesDataset;
                if (report.GroupingId == ReportGrouping.Overall)
                {
                    seriesDataset = BuildSeriesFromAggregateTotals(userIdentity.PwShop, report);
                }
                else
                {
                    seriesDataset = BuildSeriesWithGrouping(userIdentity.PwShop, report, summary);
                }

                trans.Complete();

                var flattenedSeries = new List<ReportSeries>();
                foreach (var series in seriesDataset)
                {
                    series.VisitSeries(item => flattenedSeries.Add(item));
                }

                var seriesForChart =
                    flattenedSeries.Where(x => x.Parent == null)
                        .Select(x => x.ToJsonSeries()).ToList();

                var drilldownForChart =
                    flattenedSeries.Where(x => x.Parent != null)
                        .Select(x => x.ToJsonSeries()).ToList();

                return new JsonNetResult(
                    new
                    {
                        CurrencyId = shopCurrencyId,
                        Summary = summary,
                        Series = seriesForChart,
                        Drilldown = drilldownForChart
                    });
            }
        }

        [HttpPost]
        public ActionResult Detail(
                long reportId, ReportGrouping grouping, ColumnOrdering ordering, 
                int pageNumber = 1, int pageSize = 50)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var repository = _factory.MakeReportRepository(userIdentity.PwShop);           
            var queryRepository = _factory.MakeProfitRepository(userIdentity.PwShop);
            var dataService = _factory.MakeDataService(userIdentity.PwShop);

            var report = repository.RetrieveReport(reportId);
            var queryContext = new TotalQueryContext
            {
                PwShopId = userIdentity.PwShop.PwShopId,
                PwReportId = reportId,
                StartDate = report.StartDate,
                EndDate = report.EndDate,
                Grouping = grouping,
                Ordering = ordering,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
            var totalCounts = queryRepository.RetreiveTotalCounts(queryContext);

            var totals = dataService.ProfitabilityDetails(reportId, grouping, ordering, pageNumber, pageSize);

            // Top-level series...
            var series =
                new object[]
                {
                    new {
                        name = "Profitability",
                        data = totals.Select(x =>
                            new
                            {
                                name = x.GroupingName,
                                y = x.TotalProfit,
                                drilldown = true,
                                drilldownurl = DrilldownUrlBuilder(
                                    report.PwReportId, grouping, x.GroupingKey, x.GroupingName, report.StartDate, report.EndDate),
                            }).ToList()
                    }
                };

            var shopCurrencyId = userIdentity.PwShop.CurrencyId;

            return new JsonNetResult(
                new { rows = totals, count = totalCounts, currency = shopCurrencyId, series = series });
        }


        [HttpGet]
        public FileContentResult ExportDetail(
                long reportId, ReportGrouping grouping, ColumnOrdering ordering)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var dataService = _factory.MakeDataService(userIdentity.PwShop);            
            var totals = dataService.ProfitabilityDetails(reportId, grouping, ordering, 1, 100000);

            string csv = CsvSerializer.SerializeToCsv(totals);
            return File(
                new System.Text.UTF8Encoding().GetBytes(csv), "text/csv", "ProfitabilityDetail.csv");
        }


        // Profitability Drilldown
        [HttpGet]
        public ActionResult Drilldown(
                long reportId, ReportGrouping grouping, string key, string name, DateTime start, DateTime end)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var queryRepository = _factory.MakeProfitRepository(userIdentity.PwShop);
            
            var keyFilters = new List<string>() {key};
            var periodType = (end - start).ToDefaultGranularity();

            var datePeriodTotals = 
                queryRepository.RetrieveDatePeriodTotals(
                    reportId, start, end, keyFilters, grouping, periodType);
            
            var series = 
                ReportSeriesFactory.GenerateSeriesRecursive(key, name, start, end, periodType, periodType);

            series.VisitElements(element =>
            {
                var total = datePeriodTotals.FirstOrDefault(element.MatchByGroupingAndDate);
                if (total != null)
                {
                    element.Amount = total.TotalProfit;
                }
            });

            var output =  new JsonSeries
            {
                name = name,
                data = series.Elements.Select(element =>
                    new JsonSeriesElement
                    {
                        name = element.DateLabel(),
                        y = element.Amount,
                        drilldown = (periodType == PeriodType.Day) ? null : "true",
                        drilldownurl = (periodType == PeriodType.Day) ? null :
                            DrilldownUrlBuilder(reportId, grouping, key, name, element.Start, element.End)
                    }).ToList()
            };

            return new JsonNetResult(output);
        }        


        private string DrilldownUrlBuilder(
                long reportId, ReportGrouping grouping, string key, string name, DateTime start, DateTime end)
        {
            return $"/ProfitService/Drilldown?reportId={reportId}&grouping={grouping}&key={key}&name={name}&" +
                   $"start={HttpUtility.UrlEncode(start.ToString("yyyy-MM-dd"))}&" +
                   $"end={HttpUtility.UrlEncode(end.ToString("yyyy-MM-dd"))}";
        }


        // *** The aggregated Grouped Totals, including Executive Summary
        private Summary BuildSummary(PwReport report, PwShop shop)
        {
            var queryRepository = _factory.MakeProfitRepository(shop);

            var queryContext = new TotalQueryContext
            {
                PwReportId = report.PwReportId,
                PwShopId = shop.PwShopId,
                StartDate = report.StartDate,
                EndDate = report.EndDate,
                Grouping = report.GroupingId,
                Ordering = ColumnOrdering.ProfitDescending,
            };

            var executiveSummary = queryRepository.RetreiveTotalsForAll(queryContext);

            var productTotals =
                queryRepository
                    .RetreiveTotalsByProduct(queryContext)
                    .AppendAllOthersAsDifferenceOfSummary(executiveSummary);

            var variantTotals =
                queryRepository
                    .RetreiveTotalsByVariant(queryContext)
                    .AppendAllOthersAsDifferenceOfSummary(executiveSummary);

            var productTypeTotals =
                queryRepository
                    .RetreiveTotalsByProductType(queryContext)
                    .AppendAllOthersAsDifferenceOfSummary(executiveSummary);

            var vendorTotals =
                queryRepository
                    .RetreiveTotalsByVendor(queryContext)
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

        // *** Builds Report Series data for High Charts multi-column chart
        private List<ReportSeries> BuildSeriesWithGrouping(PwShop shop, PwReport report, Summary summary)
        {
            var queryRepository = _factory.MakeProfitRepository(shop);

            // 1st-level drill down
            var periodType = (report.EndDate - report.StartDate).ToDefaultGranularity();

            //  The Grouping Names should match with those from the Summary
            var topGroups = summary
                .TotalsByGroupingId(report.GroupingId)
                .Take(NumberOfColumnGroups)
                .ToList();

            var groupingKeysAndName = topGroups
                .Select(x => new GroupingKeyAndName { GroupingKey = x.GroupingKey, GroupingName = x.GroupingName })
                .ToList();

            var groupingKeys = topGroups.Select(x => x.GroupingKey).ToList();

            // Context #1 - Grouped Data organized by Date
            var datePeriodTotals =
                RetrieveDatePeriodTotalsRecursive(
                    shop, report.PwReportId, report.StartDate, report.EndDate,
                    groupingKeys, report.GroupingId, periodType, periodType.NextDrilldownLevel());

            //foreach (var total in datePeriodTotals.Where(x => x.GroupingKey == "3D Printer"))
            //{
            //    _logger.Debug(total.ToString());
            //}

            // Context #2 - Aggregate Date Totals
            var aggregateDateTotals =
                queryRepository
                    .RetrieveDateTotals(report.PwReportId, report.StartDate, report.EndDate)
                    .ToDictionary(x => x.OrderDate, x => x);

            // Context #3 - Report Series hierarchical structure
            var seriesDataset =
                ReportSeriesFactory.GenerateSeriesMultiple(
                    report.StartDate, report.EndDate, groupingKeysAndName, periodType, periodType.NextDrilldownLevel());

            foreach (var series in seriesDataset)
            {
                series.VisitElements(element =>
                {
                    // if (element.Parent.GroupingKey == "3D Printer") _logger.Debug("Assigning Totals for " + element.ToString());
                    var total = datePeriodTotals.FirstOrDefault(element.MatchByGroupingAndDate);
                    if (total != null)
                    {
                        element.Amount = total.TotalProfit;
                        // if (element.Parent.GroupingKey == "3D Printer") _logger.Debug("Found Date Period Total " + total.ToString());
                    }
                });
            }

            if (summary.TotalsByGroupingId(report.GroupingId).Count > NumberOfColumnGroups)
            {
                // ...add the "All Other" catch-all
                var allOtherSeries = 
                    ReportSeriesFactory.GenerateSeriesRecursive(
                        AllOtherGroupingName, AllOtherGroupingName,
                        report.StartDate, report.EndDate, periodType, periodType.NextDrilldownLevel());

                allOtherSeries.VisitElements(element =>
                {
                    var totalAll = aggregateDateTotals.Total(element.Start, element.End, x => x.TotalProfit);
                    var matchingDatePeriodTotals = datePeriodTotals.Where(x => element.MatchByDate(x)).ToList();
                    element.Amount = totalAll - matchingDatePeriodTotals.Sum(x => x.TotalProfit);
                });

                seriesDataset.Add(allOtherSeries);
            }

            return seriesDataset;
        }

        // ... recursively invokes SQL to build data set of Date Totals organized by Grouping
        private List<DatePeriodTotal>  RetrieveDatePeriodTotalsRecursive(
                PwShop shop, long reportId, DateTime startDate, DateTime endDate, List<string> keyFilters,
                ReportGrouping grouping, PeriodType periodType, PeriodType maximumPeriodType)
        {
            var queryRepository = _factory.MakeProfitRepository(shop);

            var output = 
                queryRepository.RetrieveDatePeriodTotals(
                    reportId, startDate, endDate, keyFilters, grouping, periodType);

            if (periodType != PeriodType.Day && periodType != maximumPeriodType)
            {
                output.AddRange(
                    RetrieveDatePeriodTotalsRecursive(
                        shop, reportId, startDate, endDate, keyFilters, grouping,
                        periodType.NextDrilldownLevel(), maximumPeriodType));
            }

            return output;
        }

        // *** Builds Report Series data for High Charts multi-column chart
        private List<ReportSeries> BuildSeriesFromAggregateTotals(PwShop shop, PwReport report)
        {
            var queryRepository = _factory.MakeProfitRepository(shop);

            // 1st-level drill down
            var periodType = (report.EndDate - report.StartDate).ToDefaultGranularity();

            var totals = queryRepository
                .RetrieveDateTotals(report.PwReportId, report.StartDate, report.EndDate);

            var aggregateDateTotals = totals.ToDictionary(x => x.OrderDate, x => x);

            // Context #3 - Report Series hierarchical structure
            var series = ReportSeriesFactory.GenerateSeriesRecursive(
                    "All", "All", report.StartDate, report.EndDate, periodType, PeriodType.Day);
            series.VisitElements(element =>
            {                
                element.Amount = aggregateDateTotals.Total(element.Start, element.End, x=> x.TotalProfit);
            });

            return new List<ReportSeries> { series };
        }
    }
}

