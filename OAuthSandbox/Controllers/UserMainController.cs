using System.Web.Mvc;

namespace OAuthSandbox.Controllers
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

