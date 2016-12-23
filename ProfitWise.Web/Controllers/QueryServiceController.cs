﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;
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
        private readonly IPushLogger _logger;

        public const int NumberOfColumnGroups = 5;
        public const string AllOtherGroupingName = "All other";
        // Date-bucketed Totals
        public const string NoGroupingName = "All";


        public QueryServiceController(
            MultitenantFactory factory,
            IPushLogger logger)
        {
            _factory = factory;
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
        public ActionResult ProductSelectionsByPage(long reportId, int pageNumber = 1,
            int pageSize = PreviewSelectionLimit.MaximumNumberOfProducts)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportQueryRepository(userBrief.PwShop);

            var limit = PreviewSelectionLimit.MaximumNumberOfProducts;
            var selections = repository.RetrieveProductSelections(reportId, pageNumber, pageSize);
            var counts = repository.RetrieveReportRecordCount(reportId);

            return new JsonNetResult(new {Selections = selections, RecordCounts = counts});
        }

        [HttpGet]
        public ActionResult VariantSelectionsByPage(
                long reportId, int pageNumber = 1, int pageSize = PreviewSelectionLimit.MaximumNumberOfProducts)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportQueryRepository(userBrief.PwShop);

            var limit = PreviewSelectionLimit.MaximumNumberOfVariants;
            var selections = repository.RetrieveVariantSelections(reportId, pageNumber, pageSize);
            var counts = repository.RetrieveReportRecordCount(reportId);

            return new JsonNetResult(new {Selections = selections, RecordCounts = counts});
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
                var seriesDataset = new List<ReportSeries>();

                if (report.GroupingId == ReportGrouping.Overall)
                {
                    seriesDataset = BuildSeriesFromAggregateTotals(userBrief.PwShop, report);
                }
                else
                {
                    seriesDataset = BuildSeriesWithGrouping(userBrief.PwShop, report, summary);
                }

                transaction.Commit();

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


        // *** The aggregated Grouped Totals, including Executive Summary
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


        // *** Builds Report Series data for High Charts multi-column chart
        private List<ReportSeries> BuildSeriesWithGrouping(PwShop shop, PwReport report, Summary summary)
        {
            var queryRepository = _factory.MakeReportQueryRepository(shop);

            // 1st-level drill down
            var periodType = (report.EndDate - report.StartDate).ToDefaultGranularity();

            //  The Grouping Names should match with those from the Summary
            var topGroups = summary
                .TotalsByGroupingId(report.GroupingId)
                .Take(NumberOfColumnGroups)
                .ToList();

            var groupingKeysAndName = topGroups
                .Select(x => new GroupingKeyAndName {GroupingKey = x.GroupingKey, GroupingName = x.GroupingName,})
                .ToList();

            var groupingKeys = topGroups.Select(x => x.GroupingKey).ToList();

            // Context #1 - Grouped Data organized by Date
            var datePeriodTotals =
                RetrieveDatePeriodTotalsRecursive(
                    shop, report.PwReportId, report.StartDate, report.EndDate,
                    groupingKeys, report.GroupingId, periodType, periodType.NextDrilldownLevel());

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
                    var total = datePeriodTotals.FirstOrDefault(element.MatchByGroupingAndDate);
                    if (total != null)
                        element.Amount = total.TotalProfit;
                });
            }

            // All Other catch-all
            var allOtherSeries = 
                ReportSeriesFactory.GenerateSeriesRecursive(
                    "All", "All", report.StartDate, report.EndDate, periodType, periodType.NextDrilldownLevel());

            allOtherSeries.VisitElements(element =>
            {
                var totalAll = aggregateDateTotals.Total(element.Start, element.End, x => x.TotalProfit);
                var matchingDatePeriodTotals = datePeriodTotals.Where(x => element.MatchByDate(x)).ToList();
                element.Amount = totalAll - matchingDatePeriodTotals.Sum(x => x.TotalProfit);
            });

            return seriesDataset;
        }

        // ... recursively invokes SQL to build data set of Date Totals organized by Grouping
        private List<DatePeriodTotal>  RetrieveDatePeriodTotalsRecursive(
                PwShop shop, long reportId, DateTime startDate, DateTime endDate, List<string> keyFilters,
                ReportGrouping grouping, PeriodType periodType, PeriodType maximumPeriodType)
        {
            var queryRepository = _factory.MakeReportQueryRepository(shop);

            var output = queryRepository.RetrieveDatePeriodTotals(
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

        // Report Series output data type        



        // *** Builds Report Series data for High Charts multi-column chart
        private List<ReportSeries> BuildSeriesFromAggregateTotals(PwShop shop, PwReport report)
        {
            var queryRepository = _factory.MakeReportQueryRepository(shop);

            // 1st-level drill down
            var periodType = (report.EndDate - report.StartDate).ToDefaultGranularity();

            var aggregateDateTotals = queryRepository
                    .RetrieveDateTotals(report.PwReportId, report.StartDate, report.EndDate)
                    .ToDictionary(x => x.OrderDate, x => x);

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

