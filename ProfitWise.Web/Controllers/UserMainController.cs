using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    public class UserMainController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;

        public UserMainController(MultitenantFactory factory, CurrencyService currencyService)
        {
            _factory = factory;
            _currencyService = currencyService;
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


        public ActionResult BulkEditCogs(int masterProductId)
        {
            this.LoadCommonContextIntoViewBag();

            var userBrief = HttpContext.PullUserBriefFromContext();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.Shop);

            var product = cogsRepository.RetrieveProduct(masterProductId);
            product.Variants = cogsRepository.RetrieveVariants(new List<long> {masterProductId});
            product.PopulateNormalizedCogsAmount(_currencyService, userBrief.Shop.CurrencyId);

            return View(product);
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

