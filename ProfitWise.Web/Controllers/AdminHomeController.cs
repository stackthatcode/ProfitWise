using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using OAuthSandbox.Models;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminHomeController : Controller
    {
        private readonly ShopifyCredentialService _shopifyCredentialService;
        private readonly ApplicationDbContext _dbContext;

        public AdminHomeController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public AdminHomeController(ShopifyCredentialService shopifyCredentialService, ApplicationDbContext dbContext)
        {
            _shopifyCredentialService = shopifyCredentialService;
            _dbContext = dbContext;
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
            var adminRoleId = _dbContext.Roles.First(x => x.Name == SecurityConfig.AdminRole).Id;

            var credentials = _shopifyCredentialService.Retrieve(User.Identity.GetUserId());
            
            var users =
                _dbContext.Users
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
            _shopifyCredentialService.SetAdminImpersonation(currentUserId, userId);
            return RedirectToAction("Index", "UserHome");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ClearImpersonation()
        {
            var currentUserId = HttpContext.User.ExtractUserId();
            _shopifyCredentialService.ClearAdminImpersonation(currentUserId);
            return RedirectToAction("Users");
        }
    }
}
