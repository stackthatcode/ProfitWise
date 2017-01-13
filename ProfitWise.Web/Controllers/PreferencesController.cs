using System.Web.Mvc;
using ProfitWise.Data.Factories;
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

        public PreferencesController(MultitenantFactory factory)
        {
            _factory = factory;
        }

        [HttpGet]
        public ActionResult Edit()
        {
            return View();
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

