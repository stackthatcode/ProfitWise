using System;
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
                    columnChartData = BuildSeriesFromAggregateTotals(report, dateTotals);
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
        private ColumnChartData BuildSeriesWithGrouping(PwShop shop, PwReport report, Summary summary)
        {
            var queryRepository = _factory.MakeReportQueryRepository(shop);
            
            // 1st-level drill down
            var granularity = (report.EndDate - report.StartDate).ToDefaultGranularity();

            //  The Grouping Names should match with those from the Summary
            var topGroups = summary
                    .TotalsByGroupingId(report.GroupingId)
                    .Take(NumberOfColumnGroups)
                    .ToList();

            var groupingNames = topGroups.Select(x => x.GroupingName).ToList();
            var groupingKeys = topGroups.Select(x => x.GroupingKey).ToList();

            var canonizedTotals =
                BuildCanonizedDateTotals(shop, report.PwReportId, report.StartDate, report.EndDate,
                    groupingKeys, report.GroupingId, granularity, granularity.NextDrilldownLevel());
           
            var seriesDataset = 
                BuildSeriesTopLevel(
                    report.StartDate, report.EndDate, groupingNames, granularity, canonizedTotals, granularity.NextDrilldownLevel());

            var aggregageDateTotals =
               queryRepository
                   .RetrieveDateTotals(report.PwReportId, report.StartDate, report.EndDate)
                   .ToDictionary(x => x.OrderDate, x => x);

            var series = ReportSeriesFactory.GenerateSeries(
                    AllOtherGroupingName, report.StartDate, report.EndDate, granularity);

            var allOtherTotals = BuildAllOtherTotals(
                report.StartDate, report.EndDate, series, seriesDataset, aggregageDateTotals, granularity.NextDrilldownLevel());

            seriesDataset.AddRange(allOtherTotals);

            // Critical - this illustrates the boundary (of a bounded context) between 
            // a flattened List representation and Highchart's representation of data
            return new ColumnChartData
            {
                Series = seriesDataset.Where(x => x.Granularity == granularity).ToList(),
                Drilldown = seriesDataset.Where(x => x.Granularity > granularity).ToList()
            };
        }

        // ... recursively invokes SQL to build data set of Date Totals organized by Grouping
        private List<CanonizedDateTotal> BuildCanonizedDateTotals(
                PwShop shop, long reportId, DateTime startDate, DateTime endDate, List<string> groupingKeys,
                ReportGrouping grouping, DataGranularity granularity, DataGranularity maximiumGranularity)
        {
            var queryRepository = _factory.MakeReportQueryRepository(shop);
            var output = queryRepository.RetrieveCanonizedDateTotals(
                    reportId, startDate, endDate, groupingKeys, grouping, granularity);

            if (granularity != DataGranularity.Day && granularity != maximiumGranularity)
            {
                output.AddRange(
                    BuildCanonizedDateTotals(
                            shop, reportId, startDate, endDate, groupingKeys, grouping, 
                            granularity.NextDrilldownLevel(), maximiumGranularity));
            }

            return output;
        }

        private List<ReportSeries> BuildSeriesTopLevel(
                DateTime start, DateTime end, List<string> groupingNames, DataGranularity granularity,
                List<CanonizedDateTotal> canonizedTotals, DataGranularity maximumGranularity)
        {
            var seriesDataset = new List<ReportSeries>();
            foreach (var groupingName in groupingNames)
            {
                var series = GenerateSeries(groupingName, start, end, granularity, canonizedTotals);
                seriesDataset.Add(series);

                seriesDataset.AddRange(BuildDrilldown(series, canonizedTotals, maximumGranularity));
            }
            return seriesDataset;
        }

        private List<ReportSeries> BuildDrilldown(
                    ReportSeries series, List<CanonizedDateTotal> canonizedTotals, DataGranularity maximumGranularity)
        {
            var output = new List<ReportSeries>();
            if (series.Granularity >= maximumGranularity)
            {
                return output;
            }

            foreach (var element in series.data)
            {
                var granularity = (element.EndDate - element.StartDate).ToDefaultGranularity();
                var drillDownSeries = GenerateSeries(
                        series.name, element.StartDate, element.EndDate, granularity, canonizedTotals);

                drillDownSeries.id = element.CanonizedIdentifier;
                element.drilldown = element.CanonizedIdentifier;
                output.Add(drillDownSeries);

                output.AddRange(BuildDrilldown(drillDownSeries, canonizedTotals, maximumGranularity));
            }

            return output;
        }

        private ReportSeries GenerateSeries(
                    string name, DateTime start, DateTime end,  DataGranularity granularity, 
                    List<CanonizedDateTotal> canonizedDateTotals)
        {
            var series = ReportSeriesFactory.GenerateSeries(name, start, end, granularity);
            series.id = name;   // The jury is out on this one!!

            foreach (var element in series.data)
            {
                var total = canonizedDateTotals.FirstOrDefault(
                        x => x.CanonizedIdentifier == element.CanonizedIdentifier);

                if (total != null)
                    element.y = total.TotalProfit;
            }
            return series;
        }

        private List<ReportSeries> BuildAllOtherTotals(
                DateTime start, DateTime end, ReportSeries inputSeries, List<ReportSeries> seriesForComputation, 
                Dictionary<DateTime, DateTotal> dateTotals, DataGranularity maximumGranularity)
        {
            var output = new List<ReportSeries>();
            output.Add(inputSeries);

            foreach (var element in inputSeries.data)
            {
                // Find all other series elements with the same canonical date
                var matchingElements = 
                    seriesForComputation
                        .SelectMany(x => x.data)
                        .Where(x => x.DateIdentifier == element.DateIdentifier)
                        .ToList();

                var isolatedTotal = matchingElements.Sum(item => item.y);

                var overallTotal = dateTotals.Total(element.StartDate, element.EndDate, x => x.TotalProfit);
                element.y = overallTotal - isolatedTotal;
                
                if (inputSeries.Granularity < maximumGranularity)
                {
                    var nextGranularity = (element.EndDate - element.StartDate).ToDefaultGranularity();

                    var nextSeries = 
                        ReportSeriesFactory.GenerateSeries(
                            AllOtherGroupingName, element.StartDate, element.EndDate, nextGranularity);

                    // This enables Highcharts drilldown
                    element.drilldown = element.CanonizedIdentifier;
                    nextSeries.id = element.CanonizedIdentifier;

                    output.Add(nextSeries);

                    output.AddRange(
                        BuildAllOtherTotals(
                            element.StartDate, element.EndDate, nextSeries, seriesForComputation, dateTotals, maximumGranularity));
                }
            }

            return output;
        }



        // *** Aggregate Totals - no grouping with this data
        private ColumnChartData BuildSeriesFromAggregateTotals(PwReport report, Dictionary<DateTime, DateTotal> dateTotals)
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
                element.y = dateTotals.Total(element.StartDate, element.EndDate, x => x.TotalProfit);
            }

            return series;
        }
    }
}

