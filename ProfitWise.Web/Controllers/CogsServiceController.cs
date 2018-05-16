using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    [MaintenanceAttribute]
    public class CogsServiceController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;

        public CogsServiceController(MultitenantFactory factory, CurrencyService currencyService)
        {
            _factory = factory;
            _currencyService = currencyService;
        }
        

        [HttpPost]
        public ActionResult Search(CogsSearchParameters parameters)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var pickListRepository = _factory.MakePickListRepository(userIdentity.PwShop);

            long newPickListId;

            using (var trans = pickListRepository.InitiateTransaction())
            {
                if (parameters.CurrentPickListId.HasValue)
                {
                    pickListRepository.Delete(parameters.CurrentPickListId.Value);
                }

                newPickListId = pickListRepository.CreateNew();

                var terms = (parameters.Text ?? "").SplitBy(',');

                pickListRepository.Populate(newPickListId, terms);

                if (parameters.Filters != null && parameters.Filters.Count > 0)
                {
                    pickListRepository.Filter(newPickListId, parameters.Filters);

                    if (parameters.Filters.Any(x => x.Type == ProductSearchFilterType.MissingCogs))
                    {
                        pickListRepository.FilterMissingCogs(newPickListId);
                    }
                }

                trans.Commit();
            }
            
            return new JsonNetResult(new { PickListId = newPickListId});
        }

        [HttpPost]
        public ActionResult RetrieveResults(SearchResultSelection resultSelection)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);
            var pickListRepository = _factory.MakePickListRepository(userIdentity.PwShop);


            if (!pickListRepository.Exists(resultSelection.PickListId))
            {
                return new JsonNetResult(new { pickListValid = false, });
            }

            var recordCount = pickListRepository.Count(resultSelection.PickListId);

            // Pull the Search Results by Pick List page number
            var products =
                cogsRepository.RetrieveCogsSummaryFromPicklist(
                    resultSelection.PickListId,
                    resultSelection.PageNumber, 
                    resultSelection.PageSize,
                    resultSelection.SortByColumn, 
                    resultSelection.SortByDirectionDown);

            var masterProductIds = products.Select(x => x.PwMasterProductId).ToList();
            var masterVariants = cogsRepository.RetrieveVariants(masterProductIds);

            var cogsDetails = cogsRepository.RetrieveCogsDetailByMasterProduct(masterProductIds);

            foreach (var masterVariant in masterVariants)
            {
                masterVariant.PopulateCogsDetails(cogsDetails);
            }

            products.PopulateVariants(masterVariants);
            products.PopulateNormalizedCogsAmount(_currencyService, userIdentity.PwShop.CurrencyId);

            // Notice: we're using the Shop Currency to represent the price
            return new JsonNetResult(new { pickListValid = true, products, totalRecords = recordCount });
        }

        [HttpGet]
        public ActionResult RetrieveMasterProduct(long pwMasterProductId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var shopCurrencyId = userIdentity.PwShop.CurrencyId;
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            var masterProductSummary = cogsRepository.RetrieveProduct(pwMasterProductId);
            var details = cogsRepository.RetrieveCogsDetailByMasterProduct(pwMasterProductId);

            if (masterProductSummary == null)
            {
                return new JsonNetResult(new { MasterProduct = (PwCogsMasterProductModel)null });
            }

            var masterProduct = new PwCogsMasterProductModel
            {
                PwMasterProductId = masterProductSummary.PwMasterProductId,
                Title = masterProductSummary.Title,
                MasterVariants = cogsRepository.RetrieveVariants(new[] {pwMasterProductId}),
                ProductType = masterProductSummary.ProductType.IsNullOrEmptyAlt(SearchConstants.NoProductType),
                Vendor = masterProductSummary.Vendor.IsNullOrEmptyAlt(SearchConstants.NoVendor),
            };

            foreach (var variant in masterProduct.MasterVariants)
            {
                variant.PopulateNormalizedCogsAmount(_currencyService, shopCurrencyId);
                variant.PopulateCogsDetails(details);
            }

            return new JsonNetResult(new { MasterProduct = masterProduct });
        }


        // Stocked Directly and Exclude functions
        [HttpPost]
        public ActionResult StockedDirectlyByPickList(long pickListId, bool newValue)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateStockedDirectlyByPicklist(pickListId, newValue);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult StockedDirectlyByMasterProductId(long pwMasterProductId, bool value)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateStockedDirectlyByMasterProductId(pwMasterProductId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult ExcludeByPickList(long pickListId, bool value)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateExcludeByPicklist(pickListId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult ExcludeByMasterProductId(long pwMasterProductId, bool value)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateExcludeByMasterProductId(pwMasterProductId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult ExcludeByMasterVariantId(long pwMasterVariantId, bool value)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateExcludeByMasterVariantId(pwMasterVariantId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult StockedDirectlyByMasterVariantId(long pwMasterVariantId, bool value)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateStockedDirectlyByMasterVariantId(pwMasterVariantId, value);
            return JsonNetResult.Success();
        }




        [HttpPost]
        public ActionResult UpdateSimpleCogs(long pwMasterVariantId, CogsDto simpleCogs)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var service = _factory.MakeCogsService(userIdentity.PwShop);
            simpleCogs.ValidateCurrency(_currencyService); 
            service.SaveCogsForMasterVariant(pwMasterVariantId, simpleCogs, null);
            return JsonNetResult.Success();
        }        

        [HttpPost]
        public ActionResult UpdateCogsDetails(
                long pwMasterVariantId, CogsDto defaults, List<CogsDto> details)
        {
            defaults.ValidateCurrency(_currencyService);
            details = details ?? new List<CogsDto>();             
            details.ForEach(x => x.ValidateCurrency(_currencyService));

            var userIdentity = HttpContext.IdentitySnapshot();
            var service = _factory.MakeCogsService(userIdentity.PwShop);
            service.SaveCogsForMasterVariant(pwMasterVariantId, defaults, details);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult UpdateCogsDetailsPickList(
                long pwPickListId, CogsDto defaults, List<CogsDto> details)
        {
            defaults.ValidateCurrency(_currencyService);
            details = details ?? new List<CogsDto>();
            details.ForEach(x => x.ValidateCurrency(_currencyService));

            var userIdentity = HttpContext.IdentitySnapshot();
            var service = _factory.MakeCogsService(userIdentity.PwShop);
            service.SaveCogsForPickList(pwPickListId, defaults, details);

            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult UpdateAndCopyCogsDetails(
                long pwMasterVariantId, CogsDto defaults, List<CogsDto> details)
        {
            defaults.ValidateCurrency(_currencyService);
            details = details ?? new List<CogsDto>();
            details.ForEach(x => x.ValidateCurrency(_currencyService));

            var userIdentity = HttpContext.IdentitySnapshot();
            var service = _factory.MakeCogsService(userIdentity.PwShop);

            var repository = _factory.MakeProductRepository(userIdentity.PwShop);
            var masterProductId = repository.RetrieveMasterProductByMasterVariantId(pwMasterVariantId);
            service.SaveCogsForMasterProduct(masterProductId, defaults, details);
            
            return JsonNetResult.Success();
        }        
    }
}

