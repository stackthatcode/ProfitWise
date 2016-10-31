using System;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class ReportServiceController : Controller
    {
        private readonly MultitenantFactory _factory;

        public ReportServiceController(MultitenantFactory factory, CurrencyService currencyService)
        {
            _factory = factory;
        }

        [HttpGet]
        public ActionResult All()
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);   
            var reports = repository.RetrieveReportsAll();

            return new JsonNetResult(new { reports });
        }

        [HttpGet]
        public ActionResult OverallProfitability()
        {
            return new JsonNetResult(new { report = ReportFactory.OverallProfitability() });
        }

        [HttpGet]
        public ActionResult Report(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var report = repository.RetrieveReport(reportId);
            var productTypes = repository.RetrieveProductTypes(reportId);
            var vendors = repository.RetrieveVendors(reportId);
            var products = repository.RetrieveMasterProducts(reportId);
            var skus = repository.RetrieveSkus(reportId);

            return new JsonNetResult(
                new { report = report.ToReport(productTypes, vendors, products, skus) });
        }
    }
}
