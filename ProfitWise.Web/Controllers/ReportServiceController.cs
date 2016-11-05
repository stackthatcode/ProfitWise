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


        // Vendor Actions
        [HttpGet]
        public ActionResult Vendors(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var data = repository.RetrieveVendorSummary(reportId);
            return new JsonNetResult(data);
        }

        [HttpGet]
        public ActionResult SelectedVendors(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var report = repository.RetrieveReport(reportId);
            var markedVendors = repository.RetrieveMarkedVendors(reportId);

            return new JsonNetResult(new
            {
                AllVendors = report.AllVendors,
                MarkedVendors = markedVendors,
            });
        }

        [HttpPost]
        public ActionResult SelectedVendors(long reportId, bool selectAll, IList<string> markedVendors)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            using (var transaction = repository.InitiateTransaction())
            {
                if (markedVendors == null) // Strange AJAX protocol here...
                {
                    repository.ClearVendorMarks(reportId);
                    repository.UpdateSelectAllVendors(reportId, selectAll);
                }
                else
                {
                    var storedMarkedVendors = repository.RetrieveMarkedVendors(reportId);
                    var missingVendors = storedMarkedVendors.Where(x => !markedVendors.Contains(x)).ToList();
                    var newVendors = markedVendors.Where(x => !storedMarkedVendors.Contains(x)).ToList();

                    foreach (var vendor in missingVendors)
                    {
                        repository.UnmarkVendor(reportId, vendor);
                    }
                    foreach (var vendor in newVendors)
                    {
                        repository.MarkVendor(reportId, vendor);
                    }
                }

                transaction.Commit();
            }

            // TODO - clean-up the Vendor Types
            // TODO - clean-up the Master Products
            // TODO - clean-up the SKUs

            return JsonNetResult.Success();
        }


        // Master Product Actions
        [HttpGet]
        public ActionResult MasterProducts(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var data = repository.RetrieveMasterProductSummary(reportId);
            return new JsonNetResult(data);
        }

        [HttpGet]
        public ActionResult SelectedMasterProducts(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var report = repository.RetrieveReport(reportId);
            var markedMasterProducts = repository.RetrieveMarkedMasterProducts(reportId);

            return new JsonNetResult(new
            {
                AllMasterProducts = report.AllProducts,
                MarkedMasterProducts = markedMasterProducts,
            });
        }

        [HttpPost]
        public ActionResult 
                SelectedMasterProducts(
                    long reportId, bool selectAll, IList<long> markedMasterProducts)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            using (var transaction = repository.InitiateTransaction())
            {
                if (markedMasterProducts == null) // Strange AJAX protocol here...
                {
                    repository.ClearVendorMarks(reportId);
                    repository.UpdateSelectAllVendors(reportId, selectAll);
                }
                else
                {
                    var storedMarkedProducts = repository.RetrieveMarkedMasterProducts(reportId);
                    var missingProducts = storedMarkedProducts.Where(x => !markedMasterProducts.Contains(x)).ToList();
                    var newProducts = markedMasterProducts.Where(x => !storedMarkedProducts.Contains(x)).ToList();

                    foreach (var masterProduct in missingProducts)
                    {
                        repository.UnmarkMasterProduct(reportId, masterProduct);
                    }
                    foreach (var masterProduct in newProducts)
                    {
                        repository.MarkMasterProduct(reportId, masterProduct);
                    }
                }

                transaction.Commit();
            }

            // TODO - clean-up the Vendor Types
            // TODO - clean-up the Master Products
            // TODO - clean-up the SKUs

            return JsonNetResult.Success();
        }



        // Master Product Actions
        [HttpGet]
        public ActionResult Skus(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);
            var data = repository.RetrieveSkuSummary(reportId);
            return new JsonNetResult(data);
        }

        [HttpGet]
        public ActionResult SelectedSkus(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var report = repository.RetrieveReport(reportId);
            var markedSkus = repository.RetrieveMarkedSkus(reportId);

            return new JsonNetResult(new
            {
                AllSkus = report.AllSkus,
                MarkedSkus = markedSkus,
            });
        }

        [HttpPost]
        public ActionResult SelectedSkus(
                    long reportId, bool selectAll, IList<long> markedSkus)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            using (var transaction = repository.InitiateTransaction())
            {
                if (markedSkus == null) // Strange AJAX protocol here...
                {
                    repository.ClearSkuMarks(reportId);
                    repository.UpdateSelectAllSkus(reportId, selectAll);
                }
                else
                {
                    var storedMarkedSkus = repository.RetrieveMarkedSkus(reportId);
                    var missingSkus = storedMarkedSkus.Where(x => !markedSkus.Contains(x)).ToList();
                    var newSkus = markedSkus.Where(x => !storedMarkedSkus.Contains(x)).ToList();

                    foreach (var masterVariantId in missingSkus)
                    {
                        repository.UnmarkSku(reportId, masterVariantId);
                    }
                    foreach (var masterVariantId in newSkus)
                    {
                        repository.MarkSku(reportId, masterVariantId);
                    }
                }

                transaction.Commit();
            }

            return JsonNetResult.Success();
        }
    }
}

