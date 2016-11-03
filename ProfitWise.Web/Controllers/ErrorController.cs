using System;
using System.Net;
using System.Web.Mvc;
using ProfitWise.Web.Attributes;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class ErrorController : Controller
    {
        public ActionResult ServerFault(string returnUrl)
        {
            var model = new ErrorModel() {ReturnUrl = returnUrl};

            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Http404()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;

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

