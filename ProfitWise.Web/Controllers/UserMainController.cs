using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class UserMainController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;

        public UserMainController(MultitenantFactory factory, CurrencyService currencyService)
        {
            _factory = factory;
            _currencyService = currencyService;
        }

        public ActionResult Dashboard()
        {
            this.LoadCommonContextIntoViewBag();
            return View();
        }

        [HttpGet]
        public ActionResult ProductSelections(long reportId)
        {
            return View(new SimpleReport() { Id = reportId });
        }

        [HttpGet]
        public ActionResult VariantSelections(long reportId)
        {
            return View(new SimpleReport() { Id = reportId });
        }


        public ActionResult Reports()
        {
            return View();
        }

        public ActionResult Preferences()
        {
            return View();
        }
        
        

        public ActionResult Goals()
        {
            return View();
        }

        public ActionResult Products()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Ping()
        {
            return new JsonNetResult(new { Success = true });
        }
    }
}

