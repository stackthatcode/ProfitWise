using System.Web.Mvc;
using ProfitWise.Web.Models;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    public class CogsController : Controller
    {
        public CogsController()
        {
            
        }

        [HttpPost]
        public ActionResult Search(CogsSearchParameters parameters)
        {
            return new JsonNetResult(new { Success = true, Message= "hey baby!" });
        }

    }
}

