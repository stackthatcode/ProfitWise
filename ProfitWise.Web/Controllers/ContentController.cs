using System.Web.Mvc;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Web.Json;


namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class ContentController : Controller
    {
        private SystemRepository _systemRepository;

        public ContentController(SystemRepository systemRepository)
        {
            _systemRepository = systemRepository;
        }

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

        [HttpGet]
        public ActionResult MaintenanceActive()
        {
            return new JsonNetResult(
                new { Active = _systemRepository.RetrieveMaintenanceActive() });   
        }
    }
}

