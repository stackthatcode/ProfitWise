using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
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
            var checkedProductTypes = repository.RetrieveMarkedProductTypes(reportId);

            return new JsonNetResult(new
            {
                CheckedProductTypes = checkedProductTypes,
            });
        }

        [HttpPost]
        public ActionResult SelectedProductTypes(
                long reportId, IList<string> checkedProductTypes)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            using (var transaction = repository.InitiateTransaction())
            {
                if (checkedProductTypes == null) // Strange AJAX protocol for saying this is empty
                {
                    repository.ClearProductTypeMarks(reportId);
                }
                else
                {
                    var storedProductTypes = repository.RetrieveMarkedProductTypes(reportId);
                    var missingProductTypes = storedProductTypes.Where(x => !checkedProductTypes.Contains(x)).ToList();
                    var newProductTypes = checkedProductTypes.Where(x => !storedProductTypes.Contains(x)).ToList();

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

            var checkedVendors = repository.RetrieveMarkedVendors(reportId);
            return new JsonNetResult(new
            {
                CheckedVendors = checkedVendors,
            });
        }

        [HttpPost]
        public ActionResult SelectedVendors(long reportId, IList<string> checkedVendors)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            using (var transaction = repository.InitiateTransaction())
            {
                if (checkedVendors == null) // Strange AJAX protocol here...
                {
                    repository.ClearVendorMarks(reportId);
                }
                else
                {
                    var storedMarkedVendors = repository.RetrieveMarkedVendors(reportId);
                    var missingVendors = storedMarkedVendors.Where(x => !checkedVendors.Contains(x)).ToList();
                    var newVendors = checkedVendors.Where(x => !storedMarkedVendors.Contains(x)).ToList();

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

            var data = new List<PwProductSummary>();
            for (int i = 0; i < 5000; i++)
            {
                data.Add(new PwProductSummary() { PwMasterProductId = i, Title = "test", Count = 10 });
            }
            //var data = repository.RetrieveMasterProductSummary(reportId);
            return new JsonNetResult(data);
        }

        [HttpGet]
        public ActionResult SelectedMasterProducts(long reportId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            var markedMasterProducts = repository.RetrieveMarkedMasterProducts(reportId);

            return new JsonNetResult(new
            {
                MarkedMasterProducts = markedMasterProducts,
            });
        }

        [HttpPost]
        public ActionResult SelectedMasterProducts(long reportId, IList<long> markedMasterProducts)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            using (var transaction = repository.InitiateTransaction())
            {
                if (markedMasterProducts == null) // Strange AJAX protocol here...
                {
                    repository.ClearVendorMarks(reportId);
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

            var markedSkus = repository.RetrieveMarkedSkus(reportId);

            return new JsonNetResult(new { MarkedSkus = markedSkus, });
        }

        [HttpPost]
        public ActionResult SelectedSkus(long reportId, IList<long> markedSkus)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var repository = _factory.MakeReportRepository(userBrief.PwShop);

            using (var transaction = repository.InitiateTransaction())
            {
                if (markedSkus == null) // Strange AJAX protocol here...
                {
                    repository.ClearSkuMarks(reportId);
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

