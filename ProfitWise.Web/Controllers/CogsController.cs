using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Cogs;
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


        [HttpGet]
        public ActionResult Products()
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);
            
            var model = new EditProductCogsModel()
            {
                ProductTypes = cogsRepository.RetrieveProductType().ToList(),
                Vendors = cogsRepository.RetrieveVendors().ToList(),
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult CogsDetail(long? pwMasterVariantId)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            var masterVariant = cogsRepository.RetrieveVariant(pwMasterVariantId.Value);

            var defaults = new PwCogsDetail
            {
                PwMasterVariantId = pwMasterVariantId.Value,
                CogsCurrencyId = masterVariant.CogsCurrencyId,
                CogsTypeId = masterVariant.CogsTypeId,
                CogsPercentage = masterVariant.CogsPercentage,
                CogsAmount = masterVariant.CogsAmount,
               
            };
            var details = cogsRepository.RetrieveCogsDetail(pwMasterVariantId);

            var model = new CogsDetailModel
            {
                Defaults = defaults,
                Details = details,
                DateDefault = DateTime.Today,
                PwMasterVariantId = pwMasterVariantId,
            };
            return View(model);
        }


        [HttpGet]
        public ActionResult BulkEditCogs(int pwMasterProductId)
        {            
            return View(RetrieveProduct(pwMasterProductId));
        }
        
        [HttpGet]
        public ActionResult StockedPicklistPopup(int pickListId)
        {
            return View(new SimplePickList(pickListId));
        }

        [HttpGet]
        public ActionResult StockedProductPopup(int pwMasterProductId)
        {
            return View(RetrieveProduct(pwMasterProductId));
        }

        [HttpGet]
        public ActionResult ExcludedPickListPopup(int pickListId)
        {
            return View(new SimplePickList(pickListId));
        }

        [HttpGet]
        public ActionResult ExcludedProductPopup(int pwMasterProductId)
        {
            return View(RetrieveProduct(pwMasterProductId));
        }

        [HttpGet]
        public ActionResult Ping()
        {
            return new JsonNetResult(new { Success = true });
        }

        private PwCogsProductSummary RetrieveProduct(int pwMasterProductId)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            var product = cogsRepository.RetrieveProduct(pwMasterProductId);
            product.Variants = cogsRepository.RetrieveVariants(new List<long> { pwMasterProductId });
            product.PopulateNormalizedCogsAmount(_currencyService, userIdentity.PwShop.CurrencyId);

            return product;
        }
    }
}

