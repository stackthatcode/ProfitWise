using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OAuthSandbox.Attributes;
using OAuthSandbox.Models;
using Push.Utilities.Security;
using Push.Utilities.Shopify;
using Push.Utilities.Web.Helpers;
using Push.Utilities.Web.Identity;


namespace OAuthSandbox.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    public class UserHomeController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ShopifyCredentialService _shopifyCredentialService;

        public UserHomeController()
        {
        }

        public UserHomeController(
                ApplicationUserManager userManager, 
                ApplicationSignInManager signInManager,
                ShopifyCredentialService shopifyCredentialService)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            ShopifyCredentialService = shopifyCredentialService;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ShopifyCredentialService ShopifyCredentialService
        {
            get
            {
                if (_shopifyCredentialService == null)
                {
                    var context = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    _shopifyCredentialService = new ShopifyCredentialService(context);
                }
                return _shopifyCredentialService;
            }
            private set
            {
                _shopifyCredentialService = value;
            }
        }


        public async Task<ActionResult> Index()
        {
            this.ViewBag.AccessToken = "User not authenticated - no access token";

            // TODO - why does OWIN allow for this without forcing database validation...?
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && User.Identity.GetUserId() != null)
            {
                var credentials = this.ShopifyCredentialService.Retrieve(User.Identity.GetUserId());

                if (credentials == null)
                {
                    this.ViewBag.AccessToken = "User authenticated / token missing. Something is wrong - this should have redirected through Shopify authentication";
                    this.ViewBag.ShopOwner = "";
                }
                else
                {
                    this.ViewBag.AccessToken = "Access Token: " + credentials.AccessToken;
                    var impersonated = credentials.Impersonated ? "(Impersonated)" : "";
                    this.ViewBag.ShopOwner = "Shopify Owner: " + impersonated + credentials.ShopOwnerUserId;
                }
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [TokenRequired]
        public async Task<ActionResult> ShopifyOrders()
        {
            ViewBag.Message = "Hey, it looks like you're authorized!!!";

            var owinContext = System.Web.HttpContext.Current.GetOwinContext();
            var credentialsService = new ShopifyCredentialService(owinContext.GetUserManager<ApplicationUserManager>());

            var userId = HttpContext.User.ExtractUserId();

            var credentials = credentialsService.Retrieve(userId);

            var shopify_config_apikey = ConfigurationManager.AppSettings["shopify_config_apikey"];
            var shopify_config_apisecret = ConfigurationManager.AppSettings["shopify_config_apisecret"];
            var shopifyClient = 
                ShopifyHttpClient3.Factory(
                    shopify_config_apikey, shopify_config_apisecret, credentials.ShopName, credentials.AccessToken);

            var orders = shopifyClient.HttpGet("/admin/orders.json");
            ViewBag.Orders = orders.ResponseBody;

            return View();
        }

    }
}