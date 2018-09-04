using System.Web.Mvc;


namespace ProfitWise.Web.Controllers
{
    public class PublicController : Controller
    {
        public PublicController()
        {
        }
        
        [HttpGet]
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
    }
}

