using System;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Utilities.Helpers;
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

            var systemReports = repository.RetrieveSystemDefinedReports();
            var userReports = repository.RetrieveUserDefinedReports();
            userReports.AddRange(systemReports);

            return new JsonNetResult(userReports);
        }

        [HttpGet]
        public ActionResult Report(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            
            return new JsonNetResult(repository.RetrieveReport(reportId));
        }

        [HttpPost]
        public ActionResult SaveAs(long reportId, string name, bool deleteOriginal)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var filterRepository = _factory.MakeReportFilterRepository(userBrief.PwShop);

            var nameCollision = repository.ReportNameCollision(reportId, name);
            if (nameCollision)
            {
                return new JsonNetResult(new { success = false, message = "Report with same name exists already" });
            }

            var successMessage = "Report successfully saved";
            var reportToCopy = repository.RetrieveReport(reportId);
            
            if (reportToCopy.IsSystemReport)
            {
                // Looks like the User hit Save As directly from the System Report
                reportToCopy.PrepareToSavePermanent(name);
                var newReportId = repository.InsertReport(reportToCopy);
                return new JsonNetResult(new
                {
                    success = true, message = successMessage, reportId = newReportId
                });
            }

            var finalReportId = (long?) null;
            if (reportToCopy.CopyForEditing)
            {
                reportToCopy.PrepareToSavePermanent(name);
                finalReportId = reportToCopy.PwReportId;
                repository.UpdateReport(reportToCopy);
            }
            else
            {
                // The User has not yet made edits to the Report
                reportToCopy.PrepareToSavePermanent(name);
                finalReportId = repository.InsertReport(reportToCopy);
            }

            if (deleteOriginal && reportToCopy.OriginalReportId.HasValue)
            {
                // Did they opt to delete the original Report?
                repository.DeleteReport(reportToCopy.OriginalReportId.Value);
                filterRepository.DeleteFilters(reportToCopy.OriginalReportId.Value);
            }
            
            return new JsonNetResult(new
            {
                success = true, message = successMessage, reportId = finalReportId
            });
        }

        [HttpPost]
        public ActionResult Save(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var filterRepository = _factory.MakeReportFilterRepository(userBrief.PwShop);
            var reportToSave = repository.RetrieveReport(reportId);

            if (!reportToSave.CopyForEditing)
            {
                return new JsonNetResult(
                        new
                        {
                            success = true,
                            message = "No changes",
                            reportId = reportToSave.PwReportId
                        });
            }

            var originalReport = repository.RetrieveReport(reportToSave.OriginalReportId.Value);
            originalReport.Name = reportToSave.Name;
            originalReport.StartDate = reportToSave.StartDate;
            originalReport.EndDate = reportToSave.EndDate;
            originalReport.GroupingId = reportToSave.GroupingId;
            originalReport.OrderingId = reportToSave.OrderingId;

            repository.UpdateReport(originalReport);
            filterRepository.CloneFilters(reportToSave.PwReportId, originalReport.PwReportId);
            repository.DeleteReport(reportToSave.PwReportId);

            return new JsonNetResult(
                new
                {
                    success = true,
                    message = "Report successfully saved",
                    reportId = originalReport.PwReportId
                });
        }        

        [HttpPost]
        public ActionResult CopyForEditing(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var filterRepository = _factory.MakeReportFilterRepository(userBrief.PwShop);
            var reportToCopy = repository.RetrieveReport(reportId);

            reportToCopy.CopyOfSystemReport = reportId.IsSystemReport();
            reportToCopy.CopyForEditing = true;
            reportToCopy.OriginalReportId = reportId;
            var newReportId = repository.InsertReport(reportToCopy);
            filterRepository.CloneFilters(reportId, newReportId);
            filterRepository.CloneFilters(reportId, newReportId);

            var report = repository.RetrieveReport(newReportId);
            return new JsonNetResult(report);
        }

        [HttpPost]
        public ActionResult Update(
                long reportId, ReportGrouping groupingId, ReportOrdering orderingId, DateTime startDate, DateTime endDate)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var report = repository.RetrieveReport(reportId);
            report.GroupingId = groupingId;
            report.OrderingId = orderingId;
            report.StartDate = startDate;
            report.EndDate = endDate;
            report.LastAccessedDate = DateTime.Now;
            
            repository.UpdateReport(report);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult Delete(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var filterRepository = _factory.MakeReportFilterRepository(userBrief.PwShop);

            var report = repository.RetrieveReport(reportId);
            repository.DeleteReport(report.PwReportId);
            filterRepository.DeleteFilters(report.PwReportId);
            if (!report.CopyOfSystemReport && report.OriginalReportId != null)
            {
                repository.DeleteReport(report.OriginalReportId.Value);
                filterRepository.DeleteFilters(report.OriginalReportId.Value);
            }
            return JsonNetResult.Success();
        }


    }
}

