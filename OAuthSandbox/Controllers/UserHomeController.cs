using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Shopify.Factories;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    public class UserHomeController : Controller
    {
        private readonly ShopifyCredentialService _shopifyCredentialService;
        private readonly ApiRepositoryFactory _factory;

        public UserHomeController(
                ShopifyCredentialService shopifyCredentialService, 
                ApiRepositoryFactory factory)
        {
            _shopifyCredentialService = shopifyCredentialService;
            _factory = factory;
        }

        public async Task<ActionResult> Index()
        {
            this.ViewBag.AccessToken = "User not authenticated - no access token";


            // Security Testing stuff
            DefaultSecurityDataConfig.Execute(DependencyResolver.Current.GetService<ILifetimeScope>());



            // TODO - why does OWIN allow for this without forcing database validation...?
            var userId = HttpContext.User.ExtractUserId();
            var credentials = this._shopifyCredentialService.Retrieve(userId);
            
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
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Error()
        {
            throw new Exception("Oh noes! I'm throwing an Exception");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // TODO: register with Autofac
        //[TokenRequired]
        public virtual async Task<ActionResult> ShopifyOrders()
        {
            ViewBag.Message = "Hey, it looks like you're authorized!!!";

            var userId = HttpContext.User.ExtractUserId();
            var credentialServiceResults = _shopifyCredentialService.Retrieve(userId);
            if (!credentialServiceResults.Success)
            {
                throw new Exception("Shopify credentials have suddenly gone bad! Oh noes!!!");
            }

            var credentials = credentialServiceResults.ToShopifyCredentials();
            var orderApiRepository = _factory.MakeOrderApiRepository(credentials);
            var orders = orderApiRepository.Retrieve(1, 1);

            ViewBag.Orders = orders;
            return View();
        }
    }
}
