using System;
using System.Net;
using System.Web.Mvc;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Web.Attributes;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    public class ErrorController : Controller
    {
        private readonly SystemRepository _systemRepository;

        public ErrorController(SystemRepository systemRepository)
        {
            _systemRepository = systemRepository;
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult Http500(string returnUrl)
        {
            var model = new ErrorModel() {ReturnUrl = returnUrl};

            Response.ContentType = "text/html";
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Http404()
        {
            Response.ContentType = "text/html";
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;

            return View();
        }

        [HttpGet]
        public ActionResult MaintenanceActive()
        {
            return new JsonNetResult(
                new { Active = _systemRepository.RetrieveMaintenanceActive() });
        }

        [HttpGet]
        public ActionResult Maintenance()
        {
            return View();
        }


        /*** For testing purposes ***/
        [AllowAnonymous]
        public ActionResult ThrowAnonymousError()
        {
            throw new Exception(
                "This is simulation of a server fault for a non-authenticated User");
        }

        [IdentityProcessor]
        public ActionResult ThrowAuthenticatedError()
        {
            _systemRepository.NaughtySystemQuery();
            return JsonNetResult.Success();
        }
    }
}

