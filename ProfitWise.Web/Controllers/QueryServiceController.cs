﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
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
            queryRepository.GenerateQueryStub(reportId);

            // ** TODO - move this into a Service

            // Retrieve by Date Range and assign CoGS to all the Order Lines to compute Profits
            var orderLineProfits = queryRepository.RetrieveOrderLineProfits(reportId, report.StartDate, report.EndDate);
            var cogs = queryRepository.RetrieveCogsData(reportId);

            foreach (var line in orderLineProfits)
            {
                var cogsEntry = cogs.First(x => x.PwMasterVariantId == line.PwMasterVariantId);
                var normalizedCogsAmount =
                    cogsEntry.HasData
                        ? _currencyService.Convert(
                            cogsEntry.CogsAmount.Value, cogsEntry.CogsCurrencyId.Value, shopCurrencyId, line.OrderDate)
                        : 0m;

                line.PerUnitCogs = normalizedCogsAmount;
            }

            var summary = BuildExecutiveSummary(shopCurrencyId, orderLineProfits);

            return new JsonNetResult(new { summary });
        }

        // Temporary holding place for helper functions
        public PwReportExecutiveSummary BuildExecutiveSummary(int currencyId, IList<PwReportOrderLineProfit> profitLines)
        {
            return new PwReportExecutiveSummary()
            {
                CurrencyId = currencyId,
                NumberOfOrders = profitLines.Select(x => x.ShopifyOrderId).Distinct().Count(),
                CostOfGoodsSold = profitLines.Sum(x => x.TotalCogs),
                GrossRevenue = profitLines.Sum(x => x.GrossRevenue),
                Profit = profitLines.Sum(x => x.Profit)
            };
        }
    }
}

