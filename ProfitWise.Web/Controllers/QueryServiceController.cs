﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
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
                        .RetrieveDateTotals(report.PwReportId, report.StartDate, report.EndDate)
                        .ToDictionary(x => x.OrderDate, x => x);

                if (report.GroupingId == ReportGrouping.Overall)
                {
                    columnChartData = BuildSeriesOverall(report, dateTotals);
                }
                else
                {
                    columnChartData = BuildSeriesWithGrouping(userBrief.PwShop, report, summary);
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

        // Canonized Date Totals
        private ColumnChartData BuildSeriesWithGrouping(PwShop shop, PwReport report, Summary summary)
        {
            var queryRepository = _factory.MakeReportQueryRepository(shop);
            
            // 1st-level drill down
            var granularity = (report.EndDate - report.StartDate).ToDefaultGranularity();

            //  The Grouping Names should match with those from the Summary
            var topGroups = summary.TotalsByGroupingId(report.GroupingId).Take(NumberOfColumnGroups);

            var groupingNames = topGroups.Select(x => x.GroupingName).ToList();
            var groupingKeys = topGroups.Select(x => x.GroupingKey).ToList();

            var canonizedTotals =
                AssembleCanonizedDateTotals(shop, report.PwReportId, report.StartDate, report.EndDate,
                    groupingKeys, report.GroupingId, granularity, granularity.NextDrilldownLevel());

            var aggregageDateTotals =
                queryRepository
                    .RetrieveDateTotals(report.PwReportId, report.StartDate, report.EndDate)
                    .ToDictionary(x => x.OrderDate, x => x);

            var seriesDataset = BuildSeriesNonOverallTopLevel(shop, report, groupingNames, granularity);
            var drilldownDataset = BuildSeriesNonOverallDrilldown(shop, report, granularity, seriesDataset);

            // Add the all other column totals


            AppendAllOtherTotals(report.StartDate, report.EndDate, granularity, seriesDataset, dateTotals);
            //AppendAllOtherTotals(report, granularity.NextDrilldownLevel(), drilldownDataset, dateTotals);

            return new ColumnChartData { Series = seriesDataset, Drilldown = drilldownDataset };
        }


        private List<CanonizedDateTotal> AssembleCanonizedDateTotals(
                PwShop shop, long reportId, DateTime startDate, DateTime endDate, List<string> groupingKeys,
                ReportGrouping grouping, DataGranularity granularity, DataGranularity maximiumGranularity)
        {
            var queryRepository = _factory.MakeReportQueryRepository(shop);

            var output =
                queryRepository.RetrieveCanonizedDateTotals(
                    reportId, startDate, endDate, grouping, granularity);

            if (granularity != DataGranularity.Day && granularity != maximiumGranularity)
            {
                output.AddRange(
                    AssembleCanonizedDateTotals(
                        shop, reportId, startDate, endDate, groupingKeys, grouping, granularity.NextDrilldownLevel(),
                        maximiumGranularity));
            }

            return output;
        }


        private List<ReportSeries> BuildSeriesNonOverallTopLevel(
                PwShop shop, PwReport report, List<string> groupingNames, DataGranularity granularity)
        {
            var queryRepository = _factory.MakeReportQueryRepository(shop);

            var topLevelTotals =
                queryRepository.RetrieveCanonizedDateTotals(
                    report.PwReportId, report.StartDate, report.EndDate, report.GroupingId, granularity);

            var seriesDataset = new List<ReportSeries>();
            foreach (var groupingName in groupingNames)
            {
                var series =
                    BuildSeriesFromCanonizedDateTotals(
                        groupingName, report.StartDate, report.EndDate, granularity, topLevelTotals);
                seriesDataset.Add(series);
            }
            return seriesDataset;
        }

        private List<ReportSeries> BuildSeriesNonOverallDrilldown(
                PwShop shop, PwReport report, DataGranularity granularity, List<ReportSeries> seriesDataset)
        {
            var queryRepository = _factory.MakeReportQueryRepository(shop);
            var drilldownDataset = new List<ReportSeries>();

            if (granularity != DataGranularity.Day)
            {
                var drilldownGranularity = granularity.NextDrilldownLevel();
                var drilldownTotals =
                    queryRepository.RetrieveCanonizedDateTotals(
                        report.PwReportId, report.StartDate, report.EndDate, report.GroupingId, drilldownGranularity);

                foreach (var element in seriesDataset.SelectMany(x => x.data))
                {
                    var series =
                        BuildSeriesFromCanonizedDateTotals(
                            element.Parent.name, element.StartDate, element.EndDate, drilldownGranularity, drilldownTotals);

                    element.drilldown = element.CanonizedIdentifier;
                    series.id = element.CanonizedIdentifier;

                    drilldownDataset.Add(series);
                }
            }
            return drilldownDataset;
        }

        public const string AllOtherGroupingName = "All other";

        private void AppendAllOtherTotals(
                DateTime start, DateTime end, DataGranularity granularity, List<ReportSeries> seriesDataset, 
                Dictionary<DateTime, DateTotal> dateTotals)
        {
            var allOtherSeries = ReportSeriesFactory.GenerateSeries(AllOtherGroupingName, start, end, granularity);

            foreach (var allOtherElement in allOtherSeries.data)
            {
                var matchingElements = 
                    seriesDataset.SelectMany(x => x.data)
                        .Where(x => x.DateIdentifier == allOtherElement.DateIdentifier)
                        .ToList();

                var isolatedTotal = matchingElements.Sum(plot => plot.y);

                var overallTotal = 
                    dateTotals.Total(allOtherElement.StartDate, allOtherElement.EndDate, x => x.TotalProfit);

                allOtherElement.y = overallTotal - isolatedTotal;
            }

            seriesDataset.Add(allOtherSeries);
        }

        private ReportSeries BuildSeriesFromCanonizedDateTotals(
                    string groupingName, DateTime start, DateTime end, DataGranularity granularity,
                    List<CanonizedDateTotal> canonizedDateTotals)
        {
            var series = ReportSeriesFactory.GenerateSeries(groupingName, start, end, granularity);
            series.id = groupingName;   // The jury is out on this one!!

            foreach (var element in series.data)
            {
                var total = canonizedDateTotals.FirstOrDefault(
                        x => x.CanonizedIdentifier == element.CanonizedIdentifier);

                if (total != null)
                {
                    element.y = total.TotalProfit;
                }
            }

            return series;
        }


        // Date-bucketed Totals
        public const string NoGroupingName = "All";

        private ColumnChartData BuildSeriesOverall(PwReport report, Dictionary<DateTime, DateTotal> dateTotals)
        {
            var granularity = (report.EndDate - report.StartDate).ToDefaultGranularity();
            var series = 
                BuildSeriesFromDateTotals(
                    NoGroupingName, report.StartDate, report.EndDate, granularity, dateTotals);

            var drilldownSeries = BuildDrillDownFromDateTotals(series, dateTotals);

            return new ColumnChartData
            {
                Series = new List<ReportSeries> { series },
                Drilldown = drilldownSeries
            };
        }

        private List<ReportSeries> BuildDrillDownFromDateTotals(
                    ReportSeries series, Dictionary<DateTime, DateTotal> dateTotals)
        {
            var output = new List<ReportSeries>();

            foreach (var element in series.data)
            {
                var granularity = (element.EndDate - element.StartDate).ToDefaultGranularity();                

                var nextSeries =
                    BuildSeriesFromDateTotals(
                        NoGroupingName, element.StartDate, element.EndDate, granularity, dateTotals);

                element.drilldown = element.CanonizedIdentifier;
                nextSeries.id = element.drilldown;
                output.Add(nextSeries);

                if (granularity != DataGranularity.Day)
                {
                    output.AddRange(BuildDrillDownFromDateTotals(nextSeries, dateTotals));
                }
            }

            return output;
        }

        private ReportSeries BuildSeriesFromDateTotals(
                    string groupingName, DateTime start, DateTime end, 
                    DataGranularity granularity, Dictionary<DateTime, DateTotal> dateTotals)
        {
            var series = ReportSeriesFactory.GenerateSeries(groupingName, start, end, granularity);            
            foreach (var element in series.data)
            {                
                element.y = dateTotals.Total(start, end, x => x.TotalProfit);
            }

            return series;
        }
    }
}

