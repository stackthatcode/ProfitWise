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
            var model = new BulkEditAllCogsModel
            {
                DefaultMargin = 20,
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult Ping()
        {
            return new JsonNetResult(new { Success = true });
        }
    }
}

