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


        public const int NumberOfColumnGroups = 5;


        [HttpPost]
        public ActionResult Dataset(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var queryRepository = _factory.MakeReportQueryRepository(userBrief.PwShop);
            var reportRepository = _factory.MakeReportRepository(userBrief.PwShop);
            var shopCurrencyId = userBrief.PwShop.CurrencyId;

            // Retreive Report
            var report = reportRepository.RetrieveReport(reportId);

            // Transforms Report Filters into Master Variants
            queryRepository.PopulateQueryStub(reportId);

            // Retrieve by Date Range and assign CoGS to all the Order Lines to compute Profits
            var orderLineProfits = 
                queryRepository.RetrieveOrderLineProfits(
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

            // Add the CoGs data...
            PopulateCogs(orderLineProfits, searchStubs, cogs, shopCurrencyId);

            // Summar
            var summaries = orderLineProfits.BuildGroupedSummary(shopCurrencyId);

            // Generate Series for Drill Down
            List<ReportSeries> drilldown = new List<ReportSeries>();
            var drillDownLevel = 
                (report.StartDate - report.EndDate)
                    .DefaultTopDrillDownLevel();

            // A list of functions we'll use to filter Order Line Profit objects

            //if (report.GroupingId == ReportGrouping.Overall)
            //{
            //    var series = ReportSeriesFactory.GenerateSeries(summary.GroupingName, start, end, drillDownLevel);
            //    PopulateSeries(series, orderLineProfits, x => x.SearchStub.PwMasterProductId == summary.GroupingKey);
            //}

            return new JsonNetResult(new { CurrencyId = shopCurrencyId, Summary = summaries, DrillDown = drilldown});
        }




        //private List<ReportSeries> ReportSeriesByTopVendor(
        //        ReportDrillDownLevel drillDownLevel, DateTime start, DateTime end, int limit,
        //        List<Func<OrderLineProfit, bool>> filters, IList<OrderLineProfit> orderLineProfits)
        //{
        //    var drilldown = new List<ReportSeries>();
            
        //    foreach (var filter in filters)
        //    {
        //        var series = ReportSeriesFactory.GenerateSeries(summary.GroupingName, start, end, drillDownLevel);
        //        PopulateSeries(series, orderLineProfits, x => x.SearchStub.Vendor == summary.GroupingKey);

        //        drilldown.Add(series);
        //    }
        //    return drilldown;
        //}

        //private List<ReportSeries> ReportSeriesByTopProduct(
        //        ReportDrillDownLevel drillDownLevel, DateTime start, DateTime end, int limit,
        //        Summary summaries, IList<OrderLineProfit> orderLineProfits)
        //{
        //    var drilldown = new List<ReportSeries>();

        //    foreach (var summary in summaries.ProductsByMostProfitable.Take(limit).ToList())
        //    {
        //        var series = ReportSeriesFactory.GenerateSeries(summary.GroupingName, start, end, drillDownLevel);
        //        PopulateSeries(series, orderLineProfits, x => x.SearchStub.PwMasterProductId == summary.GroupingKey);

        //        drilldown.Add(series);
        //    }
        //    return drilldown;
        //}

        //private List<ReportSeries> ReportSeriesByTopProductType(
        //        ReportDrillDownLevel drillDownLevel, DateTime start, DateTime end, int limit,
        //        Summary summaries, IList<OrderLineProfit> orderLineProfits)
        //{
        //    var drilldown = new List<ReportSeries>();

        //    foreach (var summary in summaries.ProductTypeByMostProfitable.Take(limit).ToList())
        //    {
        //        var series = ReportSeriesFactory.GenerateSeries(summary.GroupingName, start, end, drillDownLevel);
        //        PopulateSeries(series, orderLineProfits, x => x.SearchStub.ProductType == summary.GroupingKey);

        //        drilldown.Add(series);
        //    }
        //    return drilldown;
        //}

        //private List<ReportSeries> ReportSeriesByTopVariant(
        //        ReportDrillDownLevel drillDownLevel, DateTime start, DateTime end, int limit,
        //        Summary summaries, IList<OrderLineProfit> orderLineProfits)
        //{
        //    var drilldown = new List<ReportSeries>();

        //    foreach (var summary in summaries.VariantByMostProfitable.Take(limit).ToList())
        //    {
        //        var series = ReportSeriesFactory.GenerateSeries(summary.GroupingName, start, end, drillDownLevel);
        //        PopulateSeries(series, orderLineProfits, x => x.SearchStub.PwMasterVariantId == summary.GroupingKey);

        //        drilldown.Add(series);
        //    }
        //    return drilldown;
        //}

        
        // Takes all Order Line Profits...
        private void PopulateSeries(
                    ReportSeries series, 
                    IList<OrderLineProfit> orderLineProfits,
                    Func<OrderLineProfit, bool> orderLineFilter)
        {
            foreach (var element in series.Data)
            {
                element.Value =
                    orderLineProfits
                        .Where(x => x.OrderDate >= element.Start &&
                                    x.OrderDate <= element.End &&
                                    orderLineFilter(x))
                        .Sum(x => x.TotalCogs);
            }
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

