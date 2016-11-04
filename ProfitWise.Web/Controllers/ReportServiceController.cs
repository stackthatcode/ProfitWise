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
        public ActionResult SelectedProductTypes(long reportId, bool selectAll, IList<string> markedProductTypes)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            using (var transaction = repository.InitiateTransaction())
            {
                if (markedProductTypes == null) // Strange AJAX protocol here...
                {
                    repository.ClearProductTypeMarks(reportId);
                    repository.UpdateSelectAllProductTypes(reportId, selectAll);
                }
                else
                {
                    var storedProductTypes = repository.RetrieveMarkedProductTypes(reportId);

                    var missingProductTypes = storedProductTypes.Where(x => !markedProductTypes.Contains(x)).ToList();
                    var newProductTypes = markedProductTypes.Where(x => !storedProductTypes.Contains(x)).ToList();

                    foreach (var productType in missingProductTypes)
                    {
                        repository.UnmarkProductType(reportId, productType);
                    }
                    foreach (var productType in newProductTypes)
                    {
                        repository.MarkProductType(reportId, productType);
                    }
                }

                transaction.Commit();
            }

            // TODO - clean-up the Vendor Types
            // TODO - clean-up the Master Products
            // TODO - clean-up the SKUs

            return JsonNetResult.Success();
        }


        // TODO - replace this with the incremental correction
        
    }
}

