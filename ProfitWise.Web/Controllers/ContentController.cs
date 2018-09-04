using System.Web.Mvc;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Web.Json;


namespace ProfitWise.Web.Controllers
{
    public class ContentController : Controller
    {
        private readonly TourService _service;

        public ContentController(TourService service)
        {
            _service = service;
        }


        [HttpGet]
        [Authorize(Roles = "ADMIN, USER")]
        [IdentityProcessor]
        public ActionResult Welcome(string returnUrl)
        {            
            return View(new WelcomeModel { ReturnUrl = returnUrl ?? GlobalConfig.BaseUrl });
        }        

        [HttpGet]
        public ActionResult About()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, USER")]
        [IdentityProcessor]
        public ActionResult ShowTour(int tourIdentifier)
        {
            var shop = HttpContext.IdentitySnapshot().PwShop;
            _service.ShowTour(shop, tourIdentifier);
            return JsonNetResult.Success();
        }
    }
}

