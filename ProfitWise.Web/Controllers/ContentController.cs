using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Services;
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
        private readonly MultitenantFactory _factory;

        public ContentController(MultitenantFactory factory)
        {
            _factory = factory;
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
    }
}

