using System;
using System.Web.Mvc;
using ProfitWise.Web.Attributes;

namespace ProfitWise.Web.Controllers
{
    public class ErrorController : Controller
    {
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

