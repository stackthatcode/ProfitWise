using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Success()
        {
            // We're getting all of this stuff from Shopify
            var authorization_code = this.Request.QueryString["code"];
            var hmac = this.Request.QueryString["hmac"];
            var timestamp = this.Request.QueryString["timestamp"];
            var nonce = this.Request.QueryString["nonce"];
            var state = this.Request.QueryString["state"];
            var shop_url = this.Request.QueryString["shop"];

            // TODO - Verification of hmac
            // TODO - Verification of shop

            //var url = string.Format("https://{0}.myshopify.com/admin/oauth/access_token", shop);
            //var rest = new Experimental.RestClient(url, "50d69dbaf54ee35929a946790d5884e4", "eb52fb7d5612e383d2e9513001a012eb");

            var config = new OAuthConfiguration { ApiKey = "50d69dbaf54ee35929a946790d5884e4", SecretKey = "eb52fb7d5612e383d2e9513001a012eb" };
            var shopifyOAuth = new ShopifyOAuth(config);
            var authorization_state = shopifyOAuth.AuthorizeClient(shop_url, authorization_code, hmac, timestamp);
            var token = authorization_state.AccessToken;

            var shopifyClient = 
                new Experimental.ShopifyHttpClient2(
                    "https://3duniverse.myshopify.com", 
                    "50d69dbaf54ee35929a946790d5884e4",
                    "eb52fb7d5612e383d2e9513001a012eb",
                    token);
            var orders = shopifyClient.HttpGet("/admin/orders.json");
            
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
    }
}