﻿using System.Web.Mvc;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    public class UserMainController : Controller
    {
        public ActionResult Dashboard()
        {
            var model = this.UserModelFactory();
            return View(model);
        }

        public ActionResult Reports()
        {
            return View();
        }

        public ActionResult Preferences()
        {
            return View();
        }

        public ActionResult EditProductCogs()
        {
            var model = this.UserModelFactory();
            return View(model);
        }

        public ActionResult CoGS()
        {
            var model = this.UserModelFactory();
            return View(model);
        }


        public ActionResult Goals()
        {
            return View();
        }

        public ActionResult Products()
        {
            return View();
        }


    }
}

