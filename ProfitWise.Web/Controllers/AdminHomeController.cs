using System;
using System.Web.Mvc;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminHomeController : Controller
    {
        private readonly IShopifyCredentialService _shopifyCredentialService;
        private readonly ApplicationSignInManager _applicationSignInManager;
        private readonly SystemRepository _systemRepository;
        private readonly AdminRepository _repository;
        private readonly CurrencyService _service;

        public AdminHomeController(
                IShopifyCredentialService shopifyCredentialService,
                ApplicationSignInManager applicationSignInManager,
                SystemRepository systemRepository,
                AdminRepository repository,
                CurrencyService service)
        {
            _shopifyCredentialService = shopifyCredentialService;
            _applicationSignInManager = applicationSignInManager;
            _systemRepository = systemRepository;
            _repository = repository;
            _service = service;
        }

        
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            AuthConfig.GlobalSignOut(_applicationSignInManager);
            return RedirectToAction("Login", "AdminAuth");
        }

        [HttpGet]
        public ActionResult Users()
        {
            var users = _repository.RetrieveUsers();
            foreach (var user in users)
            {
                user.CurrencyText = _service.CurrencyIdToAbbreviation(user.CurrencyId);
            }

            return new JsonNetResult(users);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult Impersonate(string userId)
        {
            var currentUserId = HttpContext.User.ExtractUserId();
            _shopifyCredentialService.SetAdminImpersonation(currentUserId, userId);
            return JsonNetResult.Success();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult ClearImpersonation()
        {
            var currentUserId = HttpContext.User.ExtractUserId();
            _shopifyCredentialService.ClearAdminImpersonation(currentUserId);
            return RedirectToAction("Users");
        }

        [HttpGet]
        public ActionResult Maintenance()
        {
            return new JsonNetResult(new { Active = _systemRepository.RetrieveMaintenanceActive()});
        }


        [HttpPost]
        public ActionResult Maintenance(bool active)
        {
            _systemRepository.UpdateMaintenance(active, "Not supported, yet");
            return JsonNetResult.Success();
        }
    }
}
