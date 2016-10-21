using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Web.Json;


namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class CogsController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;

        public CogsController(MultitenantFactory factory, CurrencyService currencyService)
        {
            _factory = factory;
            _currencyService = currencyService;
        }


        public ActionResult Products()
        {
            this.LoadCommonContextIntoViewBag();
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);
            
            var model = new EditProductCogsModel()
            {
                ProductTypes = cogsRepository.RetrieveProductType().ToList(),
                Vendors = cogsRepository.RetrieveVendors().ToList(),
            };

            return View(model);
        }

        public ActionResult BulkEditCogs(int masterProductId)
        {            
            return View(RetrieveProduct(masterProductId));
        }



        public ActionResult StockedPicklistPopup(int pickListId)
        {
            return View(new SimplePickList(pickListId));
        }

        public ActionResult StockedProductPopup(int masterProductId)
        {
            return View(RetrieveProduct(masterProductId));
        }



        public ActionResult ExcludedPickListPopup(int pickListId)
        {
            return View(new SimplePickList(pickListId));
        }

        public ActionResult ExcludedProductPopup(int masterProductId)
        {
            return View(RetrieveProduct(masterProductId));
        }

        [HttpGet]
        public ActionResult Ping()
        {
            return new JsonNetResult(new { Success = true });
        }


        private PwCogsProductSummary RetrieveProduct(int masterProductId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            var product = cogsRepository.RetrieveProduct(masterProductId);
            product.Variants = cogsRepository.RetrieveVariants(new List<long> { masterProductId });
            product.PopulateNormalizedCogsAmount(_currencyService, userBrief.PwShop.CurrencyId);

            return product;
        }
    }
}

