using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    [MaintenanceAttribute]
    [RequiresStoreData]
    public class ReportController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly IPushLogger _logger;

        public ReportController(MultitenantFactory factory, IPushLogger logger)
        {
            _factory = factory;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Profitability()
        {            
            return View();
        }

        [HttpGet]
        public ActionResult GoodsOnHand()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ProductVariantSelections(long reportId, string selectionType)
        {
            return View(
                new ProductVariantSelectionModel
                    { Id = reportId, SelectionType = selectionType});
        }

        [HttpGet]
        public ActionResult SaveAs(long reportId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var repository = _factory.MakeReportRepository(userIdentity.PwShop);
            var report = repository.RetrieveReport(reportId);

            var originalReport = repository.RetrieveReport(report.OriginalReportId) ?? report;
            if (report.IsSystemReport || (report.CopyForEditing && originalReport.IsSystemReport))
            {
                report.Name = repository.RetrieveAvailableDefaultName();
            }

            return View(new SaveAsModel { Current = report, Original = originalReport });
        }

        [HttpGet]
        public ActionResult Ping()
        {
            return new JsonNetResult(new { Success = true });
        }
    }
}

