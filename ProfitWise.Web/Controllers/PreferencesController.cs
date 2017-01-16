using System;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Repositories;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class PreferencesController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly PwShopRepository _shopRepository;

        public PreferencesController(
                MultitenantFactory factory, PwShopRepository shopRepository)
        {
            _factory = factory;
            _shopRepository = shopRepository;
        }

        [HttpGet]
        public ActionResult Edit()
        {
            var shop = HttpContext.PullIdentity().PwShop;
            return View(shop);
        }

        [HttpGet]
        public ActionResult BulkEditAllCogs()
        {
            var shop = HttpContext.PullIdentity().PwShop;
            var model = new BulkEditAllCogsModel { DefaultMargin = shop.DefaultMargin };
            return View(model);
        }

        [HttpPost]
        public ActionResult BulkEditAllCogs(decimal amount, bool allproducts)
        {
            var shop = HttpContext.PullIdentity().PwShop;
            //var model = new BulkEditAllCogsModel { DefaultMargin = shop.DefaultMargin };
            return JsonNetResult.Success();
        }


        [HttpGet]
        public ActionResult Ping()
        {
            return new JsonNetResult(new { Success = true });
        }


        [HttpPost]
        public ActionResult UpdateStartingDate(DateTime startDate)
        {
            var shop = HttpContext.PullIdentity().PwShop;
            if (startDate > shop.StartingDateForOrders)
            {
                throw new Exception($"User attempted to set Start Date to {startDate} for shop {shop.PwShopId}");
            }
            else
            {
                _shopRepository.UpdateStartingDateForOrders(shop.PwShopId, startDate);
                return JsonNetResult.Success();
            }
        }

        [HttpPost]
        public ActionResult UpdateDefaultMargin(bool useDefaultMargin, decimal defaultMargin)
        {
            var shop = HttpContext.PullIdentity().PwShop;
            _shopRepository.UpdateDefaultMargin(shop.PwShopId, useDefaultMargin, defaultMargin);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult UpdateDefaultDateRange(int dateRangeId)
        {
            var shop = HttpContext.PullIdentity().PwShop;
            _shopRepository.UpdateDateRangeDefault(shop.PwShopId, dateRangeId);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult UpdateProfitRealization(int profitRealizationId)
        {
            var shop = HttpContext.PullIdentity().PwShop;
            _shopRepository.UpdateProfitRealization(shop.PwShopId, profitRealizationId);
            return JsonNetResult.Success();
        }
    }
}

