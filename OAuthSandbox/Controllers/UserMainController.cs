using System.Web.Mvc;

namespace ProfitWise.Web.Controllers
{
    public class UserMainController : Controller
    {
        // GET: UserMain
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}

