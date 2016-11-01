using System;
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
            var userReports = repository.RetrieveUserDefinedReports();
            var systemReports = repository.RetrieveSystemDefinedReports();
            userReports.AddRange(systemReports);

            return new JsonNetResult(new { reports = userReports });
        }

        [HttpGet]
        public ActionResult OverallProfitability()
        {
            return new JsonNetResult(new { report = PwSystemReportFactory.OverallProfitability() });
        }

        [HttpGet]
        public ActionResult Report(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            
            var report = repository.RetrieveReport(reportId);
            report.ProductTypes = repository.RetrieveProductTypes(reportId);
            report.Vendors = repository.RetrieveVendors(reportId);
            report.MasterProductIds = repository.RetrieveMasterProducts(reportId);
            report.Skus = repository.RetrieveSkus(reportId);

            return new JsonNetResult(new { report });
        }
    }
}
