using System.Web.Mvc;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;


namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class ContentController : Controller
    {
        [HttpGet]
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
        public ActionResult Maintenance()
        {
            return View();
        }

    }
}

