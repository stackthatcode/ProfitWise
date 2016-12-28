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
    public class ReportController : Controller
    {
        private readonly MultitenantFactory _factory;

        public ReportController(MultitenantFactory factory)
        {
            _factory = factory;
        }

        public ActionResult Dashboard()
        {
            this.LoadCommonContextIntoViewBag();
            return View();
        }

        public ActionResult ProductVariantSelections(long reportId, string selectionType)
        {
            return View(
                new ProductVariantSelectionModel
                    { Id = reportId, SelectionType = selectionType});
        }

        [HttpGet]
        public ActionResult SaveAs(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
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

