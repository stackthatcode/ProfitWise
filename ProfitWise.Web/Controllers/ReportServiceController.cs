using System.Collections.Generic;
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
            report.ProductTypes = repository.RetrieveMarkedProductTypes(reportId);
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

        
        public class ProductTypeSearch
        {
            public string SearchText { get; set; }
            public bool IsShowAllSelected { get; set; }
            public IList<string> MarkedProductTypes { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        [HttpPost]
        public ActionResult ProductTypesSearch(ProductTypeSearch parameters)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            
            var results = repository.RetrieveProductTypeSummary(
                    parameters.SearchText, parameters.IsShowAllSelected, parameters.MarkedProductTypes, 
                    parameters.PageNumber, parameters.PageSize);

            var count = repository.RetrieveProductTypeCount(
                parameters.SearchText, parameters.IsShowAllSelected, parameters.MarkedProductTypes);

            return new JsonNetResult(new { results, count, });
        }

        [HttpGet]
        public ActionResult SelectedProductTypes(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var report = repository.RetrieveReport(reportId);
            var markedProductTypes = repository.RetrieveMarkedProductTypes(reportId);

            return new JsonNetResult(new
            {
                AllProductTypes = report.AllProductTypes,
                MarkedProductTypes = markedProductTypes,
            });
        }

        [HttpPost]
        public ActionResult SelectAllProductTypes(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            repository.ClearProductTypeMarks(reportId);
            repository.UpdateSelectAllProductTypes(reportId, true);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult DeselectAllProductTypes(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            repository.ClearProductTypeMarks(reportId);
            repository.UpdateSelectAllProductTypes(reportId, false);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult MarkProductType(long reportId, string productType)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            repository.MarkProductType(reportId, productType);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult UnmarkProductType(long reportId, string productType)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);            
            repository.UnmarkProductType(reportId, productType);
            return JsonNetResult.Success();
        }
    }
}

