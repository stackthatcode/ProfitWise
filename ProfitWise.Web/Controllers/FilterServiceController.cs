using System.Collections.Generic;
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
    public class FilterServiceController : Controller
    {
        private readonly MultitenantFactory _factory;

        public FilterServiceController(MultitenantFactory factory, CurrencyService currencyService)
        {
            _factory = factory;
        }


        // Product Types Actions
        [HttpGet]
        public ActionResult ProductTypes()
        {
            var userIdentity = HttpContext.PullIdentity();
            var repository = _factory.MakeReportFilterRepository(userIdentity.PwShop);
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
            var userIdentity = HttpContext.PullIdentity();
            var repository = _factory.MakeReportFilterRepository(userIdentity.PwShop);
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
            var userIdentity = HttpContext.PullIdentity();
            var repository = _factory.MakeReportFilterRepository(userIdentity.PwShop);

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
            var userIdentity = HttpContext.PullIdentity();
            var repository = _factory.MakeReportFilterRepository(userIdentity.PwShop);
            var data = repository.RetrieveSkuSummary(reportId);

            var output = data.Select(x => new
            {
                Key = x.PwMasterVariantId,
                x.VariantTitle,
                x.ProductTitle,
                x.Sku,
                Title = x.Sku.IsNullOrEmptyAlt("(No Sku)") +
                            " - " + x.VariantTitle.IsNullOrEmptyAlt("(No Variant Title)"),
                Vendor = x.Vendor,
            }).ToList();

            return new JsonNetResult(output);
        }



        [HttpGet]
        public ActionResult Filters(long reportId)
        {
            var userIdentity = HttpContext.PullIdentity();

            var reportRepository = _factory.MakeReportRepository(userIdentity.PwShop);
            reportRepository.UpdateReportLastAccessed(reportId);

            var repository = _factory.MakeReportFilterRepository(userIdentity.PwShop);
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
            var userIdentity = HttpContext.PullIdentity();

            var reportRepository = _factory.MakeReportRepository(userIdentity.PwShop);
            reportRepository.UpdateReportLastAccessed(reportId);

            var repository = _factory.MakeReportFilterRepository(userIdentity.PwShop);

            var filter = new PwReportFilter()
            {
                PwShopId = userIdentity.PwShop.PwShopId,
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
            var userIdentity = HttpContext.PullIdentity();

            var reportRepository = _factory.MakeReportRepository(userIdentity.PwShop);
            reportRepository.UpdateReportLastAccessed(reportId);

            var repository = _factory.MakeReportFilterRepository(userIdentity.PwShop);
            repository.DeleteFilter(reportId, filterType, key);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult RemoveFilterById(long reportId, long filterId)
        {
            var userIdentity = HttpContext.PullIdentity();

            var reportRepository = _factory.MakeReportRepository(userIdentity.PwShop);
            reportRepository.UpdateReportLastAccessed(reportId);

            var repository = _factory.MakeReportFilterRepository(userIdentity.PwShop);
            repository.DeleteFilterById(reportId, filterId);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult RemoveFilterByType(long reportId, int filterType)
        {
            var userIdentity = HttpContext.PullIdentity();

            var reportRepository = _factory.MakeReportRepository(userIdentity.PwShop);
            reportRepository.UpdateReportLastAccessed(reportId);

            var repository = _factory.MakeReportFilterRepository(userIdentity.PwShop);
            repository.DeleteFilters(reportId, filterType);
            return JsonNetResult.Success();
        }

    }
}

