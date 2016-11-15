using System.Collections.Generic;
using System.Linq;
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




        // Product Types Actions
        [HttpGet]
        public ActionResult ProductTypes()
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var data = repository.RetrieveProductTypeSummary();
            return new JsonNetResult(data);
        }

        [HttpGet]
        public ActionResult Vendors(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var data = repository.RetrieveVendorSummary(reportId);
            return new JsonNetResult(data);
        }
        
        [HttpGet]
        public ActionResult MasterProducts(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            //var data = new List<PwProductSummary>();
            //for (int i = 0; i < 5000; i++)
            //{
            //    data.Add(new PwProductSummary() { PwMasterProductId = i, Title = "test", Count = 10 });
            //}

            var data = repository.RetrieveMasterProductSummary(reportId);
            return new JsonNetResult(data);
        }

        [HttpGet]
        public ActionResult Skus(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var data = repository.RetrieveSkuSummary(reportId);
            return new JsonNetResult(data);
        }




        [HttpGet]
        public ActionResult Filters(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var filters = repository.RetrieveFilters(reportId);
            return new JsonNetResult(filters);
        }

        [HttpPost]
        public ActionResult AddFilter(long reportId, string filterType, long numberKey, string stringKey)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var filter = repository.InsertFilter(reportId, filterType, numberKey, stringKey);
            return new JsonNetResult(filter);
        }

        [HttpPost]
        public ActionResult RemoveFilter(long reportId, long filterId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            repository.DeleteFilter(reportId, filterId);
            return JsonNetResult.Success();
        }        
    }
}

