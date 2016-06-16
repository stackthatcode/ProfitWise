using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OAuthSandbox.Models;
using ProfitWise.Web.Plumbing;
using Push.Utilities.Security;
using Push.Utilities.Web.Helpers;
using Push.Utilities.Web.Identity;

namespace OAuthSandbox.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminHomeController : Controller
    {
        private ShopifyCredentialService _shopifyCredentialService;
        private readonly OwinServices _owinServices;

        public AdminHomeController()
        {
            _owinServices = new OwinServices(this);
        }

        public AdminHomeController(ShopifyCredentialService shopifyCredentialService)
        {
            _shopifyCredentialService = shopifyCredentialService;
        }
        

        public ShopifyCredentialService ShopifyCredentialService
        {
            get
            {
                if (_shopifyCredentialService == null)
                {
                    _shopifyCredentialService = new ShopifyCredentialService(_owinServices.UserManager);
                }
                return _shopifyCredentialService;
            }
            private set
            {
                _shopifyCredentialService = value;
            }
        }



        // GET: AdminHome
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public ActionResult Users(string message = null)
        {
            var dbContext = ApplicationDbContext.Create();
            var adminRoleId = dbContext.Roles.First(x => x.Name == SecurityConfig.AdminRole).Id;
            var credentials = this.ShopifyCredentialService.Retrieve(User.Identity.GetUserId());
            
            var users =
                dbContext.Users
                    .Where(x => x.Roles.All(role => role.RoleId != adminRoleId))
                    .ToList();

            var model = new UserListModel
            {
                Message = message ?? "",
                Users = users,
                ShopifyCredentials = credentials,
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Impersonate(string userId)
        {
            var currentUserId = HttpContext.User.ExtractUserId();
            ShopifyCredentialService.SetAdminImpersonation(currentUserId, userId);
            return RedirectToAction("Index", "UserHome");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ClearImpersonation()
        {
            var currentUserId = HttpContext.User.ExtractUserId();
            ShopifyCredentialService.ClearAdminImpersonation(currentUserId);
            return RedirectToAction("Users");
        }
    }
}
