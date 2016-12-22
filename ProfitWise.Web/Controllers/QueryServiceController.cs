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
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var queryRepository = _factory.MakeReportQueryRepository(userBrief.PwShop);


            using (var transaction = repository.InitiateTransaction())
            {
                _logger.Debug("Start - Dataset method");

                var shopCurrencyId = userBrief.PwShop.CurrencyId;
                var report = repository.RetrieveReport(reportId);

                queryRepository.PopulateQueryStub(reportId);

                // Summary for consumption by pie chart
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
                    CurrencyId = shopCurrencyId,
                    ExecutiveSummary = executiveSummary,
                    ProductsByMostProfitable = productTotals,
                    VariantByMostProfitable = variantTotals,
                    ProductTypeByMostProfitable = productTypeTotals,
                    VendorsByMostProfitable = vendorTotals,
                };

                var granularity = (report.EndDate - report.StartDate).SuggestedDataGranularity();
                var dateBucketedTotals = queryRepository
                    .RetrieveDateBucketedTotals(reportId, report.StartDate, report.EndDate, report.GroupingId, granularity);

                // Build top-level Series for the column chart
                var seriesDataset = new List<ReportSeries>();
                var completeDrillDown = report.GroupingId == ReportGrouping.Overall;
                var drilldown = new List<ReportSeries>();
                
                _logger.Debug("End - Dataset method");

                transaction.Commit();

                return new JsonNetResult(
                    new
                    {
                        Sample = dateBucketedTotals,
                        CurrencyId = shopCurrencyId,
                        Summary = summary,
                        Series = seriesDataset,
                        Drilldown = drilldown
                    });
            }
        }


        
        [Obsolete]
        private List<ReportSeries> BuildSeriesTopLevel(PwReport report, List<OrderLineProfit> orderLineProfits, Summary summary)
        {
            List<ReportSeries> seriesDataset;
            var granularity = (report.EndDate - report.StartDate).SuggestedDataGranularity();

            if (report.GroupingId == ReportGrouping.Overall)
            {
                var series =
                    ReportSeriesFactory.GenerateSeries(
                        "All", GroupingKey.Factory(ReportGrouping.Overall, null),
                        report.StartDate, report.EndDate, granularity);

                series.Populate(orderLineProfits, x => true);
                seriesDataset = new List<ReportSeries> { series };
            }
            else
            {
                var orderedTotalsBySelectedGroup =
                    summary
                        .TotalsByGroupedId(report.GroupingId)
                        .Take(NumberOfColumnGroups)
                        .ToList();

                seriesDataset = new List<ReportSeries>();

                foreach (var groupedTotal in orderedTotalsBySelectedGroup)
                {
                    seriesDataset.Add(
                            BuildSeriesFromGroupKeyInclusive(
                                    orderLineProfits, 
                                    groupedTotal.GroupingName,
                                    groupedTotal.GroupingKey,
                                    report.StartDate,
                                    report.EndDate,
                                    granularity));
                }
            }
            return seriesDataset;
        }

        [Obsolete]
        private ReportSeries
                    BuildSeriesFromGroupKeyInclusive(
                        IList<OrderLineProfit> orderLines,
                        string groupingName,
                        GroupingKey groupingKey,
                        DateTime start,
                        DateTime end,
                        DataGranularity granularity)
        {
            var series =
                ReportSeriesFactory.GenerateSeries(
                    groupingName,  // Ultimaker 2, 3D Printers, 3DU285PLARED, etc.
                    groupingKey,
                    start,
                    end,
                    granularity);

            series.id = groupingName;
            series.Populate(orderLines, x => groupingKey.MatchWithOrderLine(x));
            return series;
        }

        [Obsolete]
        private List<ReportSeries> 
                BuildSeriesDrilldown(
                        List<ReportSeries> seriesDataset, 
                        List<OrderLineProfit> orderLineProfits, 
                        bool completeDrillDown)
        {
            var output = new List<ReportSeries>();
            foreach (var series in seriesDataset)
            {
                var drilldownSeries = BuildSeriesDrilldownHelper(series, orderLineProfits);
                output.AddRange(drilldownSeries);

                // Note: there's no need to drilldown past Week
                if (completeDrillDown && series.Granularity != DataGranularity.Week && series.Granularity != DataGranularity.Day)
                {
                    // Recursive invocation
                    output.AddRange(BuildSeriesDrilldown(drilldownSeries, orderLineProfits, true));
                }
            }
            return output;
        }

        [Obsolete]
        private List<ReportSeries>
                    BuildSeriesDrilldownHelper(ReportSeries series, List<OrderLineProfit> orderLines)
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

                drilldownSeries.Populate(
                        orderLines, 
                        x => series.GroupingKey.MatchWithOrderLine(x) && 
                            x.OrderDate >= element.Start &&
                            x.OrderDate <= element.End.AddDays(1).AddSeconds(-1));

                output.Add(drilldownSeries);
            }

            return output;
        }
    }
}



