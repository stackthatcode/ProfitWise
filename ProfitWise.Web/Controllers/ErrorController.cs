using System;
using System.Web.Mvc;
using ProfitWise.Web.Attributes;

namespace ProfitWise.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult ServerFault(string returnUrl)
        {
            var model = new ErrorModel() {ReturnUrl = returnUrl};
            return View(model);
        }

        public ActionResult Http400()
        {
            return View();
        }


        /*** For testing purposes ***/
        [AllowAnonymous]
        public ActionResult ThrowAnonymousError()
        {
            throw new Exception("This is simulation of a server fault for a non-authenticated User");
        }

        [IdentityProcessor]
        public ActionResult ThrowAuthenticatedError()
        {
            throw new Exception("This is simulation of a server fault for an authenticated User");
        }
    }
}

