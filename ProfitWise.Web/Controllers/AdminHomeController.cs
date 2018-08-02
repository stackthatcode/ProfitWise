using System;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Model.Cogs;
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
        private readonly MultitenantFactory _factory;
        private readonly ShopRepository _shopRepository;
        private readonly ShopOrchestrationService _shopOrchestrationService;
        private readonly HangFireService _hangFire;

        public AdminHomeController(
                    IShopifyCredentialService shopifyCredentialService,
                    ApplicationSignInManager applicationSignInManager,
                    SystemRepository systemRepository,
                    AdminRepository repository,
                    CurrencyService service,
                    MultitenantFactory factory,
                    ShopRepository shopRepository, 
                    ShopOrchestrationService shopOrchestrationService,
                    HangFireService hangFire)
        {
            _shopifyCredentialService = shopifyCredentialService;
            _applicationSignInManager = applicationSignInManager;
            _systemRepository = systemRepository;
            _repository = repository;
            _service = service;
            _factory = factory;
            _shopRepository = shopRepository;
            _shopOrchestrationService = shopOrchestrationService;
            _hangFire = hangFire;
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

        [HttpGet]
        public ActionResult User(string userId)
        {
            var user = _repository.RetrieveUser(userId);
            user.CurrencyText = _service.CurrencyIdToAbbreviation(user.CurrencyId);
            var shop = _shopRepository.RetrieveByUserId(userId);
            var billingRepository = _factory.MakeBillingRepository(shop);
            var billing = billingRepository.RetrieveAll();
            
            return new JsonNetResult(new { user, billing });
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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult CancelCharge(int shopId, long pwChargeId)
        {
            var shop = _shopRepository.RetrieveByShopId(shopId);
            _shopOrchestrationService.CancelCharge(shop.ShopOwnerUserId, pwChargeId);            
            return JsonNetResult.Success();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult ActivateCharge(int shopId, long pwChargeId)
        {
            var shop = _shopRepository.RetrieveByShopId(shopId);
            _shopOrchestrationService.ActivateCharge(shop.ShopOwnerUserId, pwChargeId);
            return JsonNetResult.Success();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult TempFreeTrialOverride(string userId, int? days)
        {
            var shop = _shopRepository.RetrieveByUserId(userId);
            _shopRepository.UpdateTempFreeTrialOverride(shop.PwShopId, days);
            return JsonNetResult.Success();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult RecreateLedger(string userId)
        {
            var shop = _shopRepository.RetrieveByUserId(userId);
            var repository = _factory.MakeCogsDownstreamRepository(shop);
            repository.DeleteEntryLedger(new EntryRefreshContext());
            repository.CreateEntryLedger(new EntryRefreshContext());
            repository.ExecuteRemoveEntriesForCancelledOrders();

            return JsonNetResult.Success();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult KillBatchJobs(string userId)
        {
            _hangFire.KillInitialRefresh(userId);
            _hangFire.KillRoutineRefresh(userId);
            return JsonNetResult.Success();
        }
        
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult KillUploadsJobs(string userId)
        {
            var shop = _shopRepository.RetrieveByUserId(userId);
            _hangFire.ZombieAllProcessingUploads(shop.PwShopId);
            return JsonNetResult.Success();
        }



        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult ScheduleInitialRefresh(string userId)
        {
            _hangFire.AddOrUpdateInitialShopRefresh(userId);
            return JsonNetResult.Success();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult ScheduleRoutineRefresh(string userId)
        {
            var result = _hangFire.AddOrUpdateRoutineShopRefresh(userId);
            return JsonNetResult.Success();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult SingleOrderRefresh(string userId, long orderId)
        {
            _hangFire.ScheduleOneTimeOrderRefresh(userId, orderId);
            return JsonNetResult.Success();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult ForceAllProductsRefresh(string userId)
        {
            var pwShop = _shopRepository.RetrieveByUserId(userId);
            var batchRepository = _factory.MakeBatchStateRepository(pwShop);
            var state = batchRepository.Retrieve();
            state.ProductsLastUpdated = null;
            batchRepository.Update(state);
            return JsonNetResult.Success();
        }


        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult Uninstall(int shopId)
        {
            var pwShop = _shopRepository.RetrieveByShopId(shopId);
            _shopOrchestrationService.UninstallShop(pwShop.ShopifyShopId);
            return JsonNetResult.Success();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult FinalizeUninstall(int shopId)
        {
            _shopOrchestrationService.FinalizeUninstallation(shopId);
            return JsonNetResult.Success();
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

