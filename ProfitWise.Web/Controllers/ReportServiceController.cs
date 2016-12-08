﻿using System;
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

            var nameCollision = repository.ReportNameCollision(reportId, name) != null;
            if (nameCollision)
            {
                return new JsonNetResult(new { success = false, message = "Report with same name exists already" });
            }

            var successMessage = "Report successfully saved";
            var sourceReport = repository.RetrieveReport(reportId);            
            if (sourceReport.CopyOfSystemReport)
            {
                // Simple case, nothing to do but update
                sourceReport.CopyForEditing = false;
                sourceReport.CopyOfSystemReport = false;
                sourceReport.LastAccessedDate = DateTime.Now;
                sourceReport.Name = name;
                var finalReportId = sourceReport.PwReportId;
                repository.UpdateReport(sourceReport);
                return new JsonNetResult(new { success = true, message = successMessage, reportId = finalReportId });
            }
            if (sourceReport.IsSystemReport)
            {
                var reportToCopy = repository.RetrieveReport(reportId);
                reportToCopy.CopyOfSystemReport = false;
                reportToCopy.CopyForEditing = false;
                reportToCopy.Name = name;
                reportToCopy.OriginalReportId = reportId;
                var finalReportId = repository.InsertReport(reportToCopy);
                return new JsonNetResult(new { success = true, message = successMessage, reportId = finalReportId });
            }

            // Is not a copy of a 
            if (deleteOriginal)
            {
                repository.DeleteReport(sourceReport.OriginalReportId);
                sourceReport.CopyForEditing = false;
                sourceReport.LastAccessedDate = DateTime.Now;
                sourceReport.Name = name;
                var finalReportId = sourceReport.PwReportId;
                repository.UpdateReport(sourceReport);
                return new JsonNetResult(new { success = true, message = successMessage, reportId = finalReportId });
            }
            else
            {
                var finalReportId = SaveChangesToOriginalReport(sourceReport);
                return new JsonNetResult(new { success = true, message = successMessage, reportId = finalReportId });
            }
        }

        [HttpPost]
        public ActionResult Save(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var reportEditCopy = repository.RetrieveReport(reportId);

            if (!reportEditCopy.CopyForEditing)
            {
                return
                    new JsonNetResult(new {success = true, message = "No changes", reportId = reportEditCopy.PwReportId});
            }
            else
            {
                var originalReportId = SaveChangesToOriginalReport(reportEditCopy);
                return new JsonNetResult(
                    new { success = true, message = "Report successfully saved", reportId = originalReportId });
            }
        }

        public long SaveChangesToOriginalReport(PwReport reportEditCopy)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var originalReport = repository.RetrieveReport(reportEditCopy.OriginalReportId);
            originalReport.Name = reportEditCopy.Name;
            originalReport.StartDate = reportEditCopy.StartDate;
            originalReport.EndDate = reportEditCopy.EndDate;
            originalReport.GroupingId = reportEditCopy.GroupingId;
            originalReport.OrderingId = reportEditCopy.OrderingId;
            repository.UpdateReport(reportEditCopy);

            repository.CloneFilters(reportEditCopy.PwReportId, originalReport.PwReportId);
            return originalReport.PwReportId;
        }

        [HttpPost]
        public ActionResult CopyForEditing(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);            
            var reportToCopy = repository.RetrieveReport(reportId);

            reportToCopy.CopyOfSystemReport = reportId.IsSystemReport();
            reportToCopy.CopyForEditing = true;
            reportToCopy.OriginalReportId = reportId;
            var newReportId = repository.InsertReport(reportToCopy);
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

            // NOTE: this is domain logic living on the controller...
            var output = data.Select(x => new
            {
                Key = x.ProductType,
                Title = x.ProductType.IsNullOrEmptyAlt("(No Product Type)"),
                ProductCount = x.Count,
            }).ToList();

            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult Vendors(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var data = repository.RetrieveVendorSummary(reportId);

            // NOTE: this is domain logic living on the controller...
            var output = data.Select(x => new
            {
                Key = x.Vendor,
                Title = x.Vendor.IsNullOrEmptyAlt("(No Vendor)"),
                ProductCount = x.Count,
            }).ToList();

            return new JsonNetResult(output);
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

            var output = data.Select(x => new
            {
                Key = x.PwMasterProductId,
                Title = x.Title.IsNullOrEmptyAlt("(No Product Title)"),
                Vendor = x.Vendor,
                VariantCount = x.VariantCount,
            }).ToList();

            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult Skus(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var data = repository.RetrieveSkuSummary(reportId);

            var output = data.Select(x => new
            {
                Key = x.PwMasterVariantId,
                x.VariantTitle,
                x.ProductTitle,
                x.Sku,
                Title =  x.Sku.IsNullOrEmptyAlt("(No Sku)") + 
                            " - " + x.VariantTitle.IsNullOrEmptyAlt("(No Variant Title)"),
                Vendor = x.Vendor,
            }).ToList();

            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult RecordCounts(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var output = repository.RetrieveReportRecordCount(reportId);            
            return new JsonNetResult(output);
        }

        [HttpGet]
        public ActionResult ProductSelectionsByPage(long reportId, int pageNumber = 1, int pageSize = PreviewSelectionLimit.MaximumNumberOfProducts)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var limit = PreviewSelectionLimit.MaximumNumberOfProducts;
            var selections = repository.RetrieveProductSelections(reportId, pageNumber, pageSize);
            var counts = repository.RetrieveReportRecordCount(reportId);

            return new JsonNetResult(new { Selections = selections, RecordCounts = counts });
        }

        [HttpGet]
        public ActionResult VariantSelectionsByPage(long reportId, int pageNumber = 1, int pageSize = PreviewSelectionLimit.MaximumNumberOfProducts)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var limit = PreviewSelectionLimit.MaximumNumberOfVariants;
            var selections = repository.RetrieveVariantSelections(reportId, pageNumber, pageSize);
            var counts = repository.RetrieveReportRecordCount(reportId);

            return new JsonNetResult(new { Selections = selections, RecordCounts = counts });
        }


        [HttpGet]
        public ActionResult Filters(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var filters = repository.RetrieveFilters(reportId);

            var output = filters.Select(x => new
            {
                PwReportId = x.PwReportId,
                PwFilterId = x.PwFilterId,
                Title = x.Title,
                Key = x.UsesNumberKey ? x.NumberKey.ToString() : x.StringKey,
                Description = x.Description,
                FilterType = x.FilterType,
            }).ToList();

            return new JsonNetResult(output);
        }

        [HttpPost]
        public ActionResult AddFilter(long reportId, int filterType, string key, string title)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var filter = new PwReportFilter()
            {
                PwShopId = userBrief.PwShop.PwShopId,
                PwReportId = reportId,
                FilterType = filterType,
                Title = title.Truncate(100),
            };

            filter.Description = filter.DescriptionBuilder();
            filter.SetKeyFromExternal(key);

            var savedFilter = repository.InsertFilter(filter);
            return new JsonNetResult(savedFilter);
        }

        [HttpPost]
        public ActionResult RemoveFilter(long reportId, int filterType, string key)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            repository.DeleteFilter(reportId, filterType, key);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult RemoveFilterById(long reportId, long filterId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            repository.DeleteFilterById(reportId, filterId);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult RemoveFilterByType(long reportId, int filterType)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            repository.DeleteFilters(reportId, filterType);
            return JsonNetResult.Success();

        }
    }
}

