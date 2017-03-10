using System;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    [MaintenanceAttribute]
    public class PreferencesController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly ShopRepository _shopRepository;
        private readonly IPushLogger _logger;
        private readonly TimeZoneTranslator _timeZoneTranslator;

        public PreferencesController(
                MultitenantFactory factory, 
                ShopRepository shopRepository, 
                IPushLogger logger,
                TimeZoneTranslator timeZoneTranslator)
        {
            _factory = factory;
            _shopRepository = shopRepository;
            _logger = logger;
            _timeZoneTranslator = timeZoneTranslator;
        }

        [HttpGet]
        public ActionResult Edit()
        {
            var shop = HttpContext.IdentitySnapshot().PwShop;
            return View(shop);
        }
        

        [HttpGet]
        public ActionResult Ping()
        {
            return new JsonNetResult(new { Success = true });
        }


        [HttpPost]
        public ActionResult UpdateStartingDate(DateTime startDate)
        {
            var shop = HttpContext.IdentitySnapshot().PwShop;

            // Translate what the User was seeing in the UI to Server Time Zone
            var translatedStartDate = _timeZoneTranslator.ToServerTime(startDate, shop.TimeZone);

            if (translatedStartDate > shop.StartingDateForOrders)
            {
                throw new Exception($"User attempted to set Start Date to {translatedStartDate} for shop {shop.PwShopId}");
            }
            else
            {
                _shopRepository.UpdateStartingDateForOrders(shop.PwShopId, translatedStartDate);
                return JsonNetResult.Success();
            }
        }

        [HttpPost]
        public ActionResult UpdateDefaultMargin(bool useDefaultMargin, decimal defaultMargin)
        {
            var shop = HttpContext.IdentitySnapshot().PwShop;
            _shopRepository.UpdateDefaultMargin(shop.PwShopId, useDefaultMargin, defaultMargin);

            // Update the in-memory copy thereof...
            shop.UseDefaultMargin = useDefaultMargin;
            shop.DefaultMargin = defaultMargin;
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult UpdateDefaultDateRange(int dateRangeId)
        {
            var shop = HttpContext.IdentitySnapshot().PwShop;
            _shopRepository.UpdateDateRangeDefault(shop.PwShopId, dateRangeId);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult UpdateProfitRealization(int profitRealizationId)
        {
            var shop = HttpContext.IdentitySnapshot().PwShop;
            _shopRepository.UpdateProfitRealization(shop.PwShopId, profitRealizationId);
            shop.ProfitRealization = profitRealizationId; // Need to update the in-memory value!
            return JsonNetResult.Success();
        }


        [HttpGet]
        public ActionResult StoreDataReady()
        {
            var storeDataLoaded = this.HttpContext.IdentitySnapshot().PwShop.IsDataLoaded;
            return new JsonNetResult(new { storeDataLoaded });
        }


        //[Obsolete]
        //[HttpGet]
        //public ActionResult OneTimeDataFix()
        //{
        //    var service = _factory.MakeCogsService(HttpContext.PullIdentity().PwShop);
        //    service.OneTimeCogsDataFixer();
        //    return JsonNetResult.Success();
        //}
    }
}

