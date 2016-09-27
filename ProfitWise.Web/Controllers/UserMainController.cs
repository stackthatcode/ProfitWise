using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    public class UserMainController : Controller
    {
        private readonly MultitenantFactory _factory;

        public UserMainController(MultitenantFactory factory)
        {
            _factory = factory;
        }

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

            var userBrief = HttpContext.PullUserBriefFromContext();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.Shop);

            var model = new EditProductCogsModel()
            {
                ProductTypes = cogsRepository.RetrieveProductType().ToList(),
                Vendors = cogsRepository.RetrieveVendors().ToList(),
            };

            return View(model);
        }


        public ActionResult BulkEditProductVariantCogs(int shopifyProductId)
        {

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

