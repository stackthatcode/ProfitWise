using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Web.Json;


namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    [MaintenanceAttribute]
    [RequiresStoreData]
    public class CogsController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;
        private readonly TimeZoneTranslator _timeZoneTranslator;

        public CogsController(
                MultitenantFactory factory, 
                CurrencyService currencyService, 
                TimeZoneTranslator timeZoneTranslator)
        {
            _factory = factory;
            _currencyService = currencyService;
            _timeZoneTranslator = timeZoneTranslator;
        }


        [HttpGet]
        public ActionResult Products()
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);
            
            var model = new EditProductCogsModel()
            {
                ProductTypes = cogsRepository.RetrieveProductType().ToList(),
                Vendors = cogsRepository.RetrieveVendors().ToList(),
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult CogsDetail(long? pwMasterVariantId, long? pwPickListId)
        {
            if (!pwMasterVariantId.HasValue && !pwPickListId.HasValue)
            {
                throw new Exception("Missing parameters");
            }

            var userIdentity = HttpContext.IdentitySnapshot();

            if (pwMasterVariantId.HasValue)
            {
                var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);
                var masterVariant = cogsRepository.RetrieveVariant(pwMasterVariantId.Value);

                var defaults = new PwCogsDetail
                {
                    PwShopId = userIdentity.PwShop.PwShopId,
                    PwMasterVariantId = pwMasterVariantId.Value,
                    CogsCurrencyId = masterVariant.CogsCurrencyId,
                    CogsTypeId = masterVariant.CogsTypeId,
                    CogsMarginPercent = masterVariant.CogsMarginPercent,
                    CogsAmount = masterVariant.CogsAmount,

                };
                var details = cogsRepository.RetrieveCogsDetailByMasterVariant(pwMasterVariantId.Value);

                var model = new CogsDetailModel
                {
                    Defaults = defaults,
                    Details = details,
                    DateDefault = _timeZoneTranslator.Today(userIdentity.PwShop.TimeZone),
                    PwMasterVariantId = pwMasterVariantId,
                };
                return View(model);
            }
            else
            {
                return View(new CogsDetailModel
                {
                    PwPickListId = pwPickListId.Value,
                    DateDefault = _timeZoneTranslator.Today(userIdentity.PwShop.TimeZone),
                });
            }
        }
        

        // Search pop-up
        [HttpGet]
        public ActionResult ProductConsolidationSearch(long pwMasterProductId)
        {
            var model = new ProductConsolidationSearchModel {PwMasterProductId = pwMasterProductId};
            return View(model);
        }

        [HttpGet]
        public ActionResult AddVariantToConsolidation(long pwMasterVariantId, long pwMasterProductId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            var masterVariants =
                cogsRepository.RetrieveVariants(new[] {pwMasterProductId})
                    .Where(x => x.PwMasterVariantId != pwMasterVariantId)
                    .ToList();

            var model = new AddVariantToConsolidationModel
            {
                PwMasterVariantId = pwMasterVariantId,
                MasterVariants = masterVariants,
            };
            
            return View(model);
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
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            var product = cogsRepository.RetrieveProduct(pwMasterProductId);
            product.Variants = cogsRepository.RetrieveVariants(new List<long> { pwMasterProductId });
            product.PopulateNormalizedCogsAmount(_currencyService, userIdentity.PwShop.CurrencyId);

            return product;
        }


        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult UploadTemplate()
        {
            throw new NotImplementedException();
        }
    }
}

