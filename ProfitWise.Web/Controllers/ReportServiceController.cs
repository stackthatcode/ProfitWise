using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
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

            return new JsonNetResult(userReports);
        }

        [HttpGet]
        public ActionResult Report(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            
            var report = repository.RetrieveReport(reportId);
            report.ProductTypes = repository.RetrieveSelectedProductTypes(reportId);
            report.Vendors = repository.RetrieveSelectedVendors(reportId);
            report.MasterProductIds = repository.RetrieveMasterProducts(reportId);
            report.Skus = repository.RetrieveSkus(reportId);

            return new JsonNetResult(report);
        }

        [HttpPost]
        public ActionResult CopyAndEdit(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var originalReport = repository.RetrieveReport(reportId);
            var newReportId = repository.CopyReport(originalReport);
            var report = repository.RetrieveReport(newReportId);
            return new JsonNetResult(report);
        }

        [HttpGet]
        public ActionResult ProductTypes()
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var data = repository.RetrieveProductTypeSummary();
            return new JsonNetResult(data);
        }

        [HttpGet]
        public ActionResult SelectedProductTypes(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var report = repository.RetrieveReport(reportId);
            var selected = repository.RetrieveSelectedProductTypes(reportId);
            return new JsonNetResult(new
            {
                AllProductTypes = report.AllProductTypes,
                SelectedProductTypes = selected
            });
        }

        [HttpPost]
        public ActionResult SelectProductType(long reportId, string productType)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            repository.SelectProductType(reportId, productType);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult DeselectProductType(long reportId, string productType)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var report = repository.RetrieveReport(reportId);
            if (report.AllProductTypes)
            {
                repository.CopyAllProductTypesToSelection(reportId);
            }
            repository.DeselectProductType(reportId, productType);
            return JsonNetResult.Success();
        }

    }
}
