using System;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Reports;
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

        public const int MaximumUserDefinedReports = 20;


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

            var current = repository.RetrieveReport(reportId);
            var original = repository.RetrieveReport(current.OriginalReportId);
            return new JsonNetResult(new { current = current, original = original ?? current });
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
                return new JsonNetResult(
                    new { success = false, message = "Report with same name exists already" });
            }

            var numberOfCustomerReports = repository.RetrieveUserDefinedReportCount();
            if (deleteOriginal == false && numberOfCustomerReports >= MaximumUserDefinedReports)
            {
                return new JsonNetResult(
                    new {success = false, message = "Maximum number of custom reports exceeded"});
            }

            var successMessage = "Report successfully saved";
            var sourceReport = repository.RetrieveReport(reportId);            

            var finalReportId = (long?) null;
            if (sourceReport.CopyForEditing)
            {
                sourceReport.PrepareToSavePermanent(name);
                finalReportId = sourceReport.PwReportId;
                repository.UpdateReport(sourceReport);

                if (deleteOriginal)
                {
                    var originalReport = repository.RetrieveReport(sourceReport.OriginalReportId);
                    if (!originalReport.IsSystemReport)
                    {
                        repository.DeleteReport(originalReport.PwReportId);
                        filterRepository.DeleteFilters(originalReport.PwReportId);
                    }
                }
            }
            else
            {
                // This is not an edit copy, therefore, we'll make a copy of it
                var copy = sourceReport.MakeCopyForEditing();
                copy.PrepareToSavePermanent(name);
                finalReportId = repository.InsertReport(copy);

                filterRepository.CloneFilters(sourceReport.PwReportId, finalReportId.Value);

                if (deleteOriginal && !sourceReport.IsSystemReport)
                {
                    repository.DeleteReport(sourceReport.PwReportId);
                    filterRepository.DeleteFilters(sourceReport.PwReportId);

                }
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

            var originalReport = repository.RetrieveReport(reportToSave.OriginalReportId);
            originalReport.Name = reportToSave.Name;
            originalReport.StartDate = reportToSave.StartDate;
            originalReport.EndDate = reportToSave.EndDate;
            originalReport.GroupingId = reportToSave.GroupingId;
            originalReport.OrderingId = reportToSave.OrderingId;

            repository.UpdateReport(originalReport);
            filterRepository.CloneFilters(reportToSave.PwReportId, originalReport.PwReportId);

            repository.DeleteReport(reportToSave.PwReportId);
            filterRepository.DeleteFilters(reportToSave.PwReportId);

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

            var sourceReport = repository.RetrieveReport(reportId);
            var copyReport = sourceReport.MakeCopyForEditing();
            copyReport.PwReportId = repository.InsertReport(copyReport);
            
            filterRepository.CloneFilters(reportId, copyReport.PwReportId);

            return new JsonNetResult(new { current = copyReport, original = sourceReport});
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

            if (report.CopyForEditing)
            {
                var originalReport = repository.RetrieveReport(report.OriginalReportId);
                if (!originalReport.IsSystemReport)
                {
                    repository.DeleteReport(originalReport.PwReportId);
                    filterRepository.DeleteFilters(originalReport.PwReportId);
                }
            }

            return JsonNetResult.Success();
        }
    }
}

