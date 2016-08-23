using System.Web.Mvc;
using ProfitWise.Web.Models;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    public class UserMainController : Controller
    {
        public ActionResult Dashboard()
        {
            this.LoadCommonContextIntoViewBag();
            return View();
        }

        public ActionResult Reports()
        {
            return View();
        }

        public ActionResult Preferences()
        {
            return View();
        }

        public ActionResult EditProductCogs()
        {
            this.LoadCommonContextIntoViewBag();
            return View();
        }

        public ActionResult BulkEditProductVariantCogs(int shopifyProductId)
        {
            this.LoadCommonContextIntoViewBag();

            var model = new SimpleShopifyProductId()
            {
                ShopifyProductId = shopifyProductId
            };
            return View(model);
        }

        public ActionResult StockedDirectlyVariantsPopup(int shopifyProductId)
        {
            this.LoadCommonContextIntoViewBag();

            var model = new SimpleShopifyProductId()
            {
                ShopifyProductId = shopifyProductId
            };
            return View(model);
        }

        public ActionResult StockedDirectlyProductsPopup()
        {
            this.LoadCommonContextIntoViewBag();
            return View();
        }



        public ActionResult ExcludedVariantsPopup(int shopifyProductId)
        {
            this.LoadCommonContextIntoViewBag();

            var model = new SimpleShopifyProductId()
            {
                ShopifyProductId = shopifyProductId
            };
            return View(model);
        }

        public ActionResult ExcludedProductsPopup()
        {
            this.LoadCommonContextIntoViewBag();
            return View();
        }


        public ActionResult Goals()
        {
            return View();
        }

        public ActionResult Products()
        {
            return View();
        }


    }
}

