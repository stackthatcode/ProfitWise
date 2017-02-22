using System;
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
            var userIdentity = HttpContext.PullIdentity();
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
            var userIdentity = HttpContext.PullIdentity();
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

            products.PopulateVariants(
                cogsRepository
                    .RetrieveVariants(
                        products.Select(x => x.PwMasterProductId).ToList(), primaryOnly: false));

            products.PopulateNormalizedCogsAmount(_currencyService, userIdentity.PwShop.CurrencyId);

            // Notice: we're using the Shop Currency to represent the price
            return new JsonNetResult(new { pickListValid = true, products, totalRecords = recordCount });
        }

        [HttpGet]
        public ActionResult RetrieveMasterProduct(long pwMasterProductId)
        {
            var userIdentity = HttpContext.PullIdentity();
            var shopCurrencyId = userIdentity.PwShop.CurrencyId;
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            var masterProductSummary = cogsRepository.RetrieveProduct(pwMasterProductId);
            if (masterProductSummary == null)
            {
                return new JsonNetResult(new { MasterProduct = (PwCogsMasterProductModel)null });
            }            

            var masterProduct = new PwCogsMasterProductModel()
            {
                PwMasterProductId = masterProductSummary.PwMasterProductId,
                Title = masterProductSummary.Title,
            };

            masterProduct.MasterVariants = cogsRepository.RetrieveVariants(new[] { pwMasterProductId });            
            foreach (var variant in masterProduct.MasterVariants)
            {
                variant.PopulateNormalizedCogsAmount(_currencyService, shopCurrencyId);
            }

            return new JsonNetResult(new { MasterProduct = masterProduct });
        }


        // Stocked Directly and Exclude functions
        [HttpPost]
        public ActionResult StockedDirectlyByPickList(long pickListId, bool newValue)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateStockedDirectlyByPicklist(pickListId, newValue);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult StockedDirectlyByMasterProductId(long pwMasterProductId, bool value)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateStockedDirectlyByMasterProductId(pwMasterProductId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult ExcludeByPickList(long pickListId, bool value)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateExcludeByPicklist(pickListId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult ExcludeByMasterProductId(long pwMasterProductId, bool value)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateExcludeByMasterProductId(pwMasterProductId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult ExcludeByMasterVariantId(long pwMasterVariantId, bool value)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateExcludeByMasterVariantId(pwMasterVariantId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult StockedDirectlyByMasterVariantId(long pwMasterVariantId, bool value)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateStockedDirectlyByMasterVariantId(pwMasterVariantId, value);
            return JsonNetResult.Success();
        }




        [HttpPost]
        public ActionResult UpdateSimpleCogs(long pwMasterVariantId, CogsDto simpleCogs)
        {
            var userIdentity = HttpContext.PullIdentity();
            var service = _factory.MakeCogsService(userIdentity.PwShop);
            simpleCogs.ValidateCurrency(_currencyService); 

            service.UpdateSimpleCogs(pwMasterVariantId, simpleCogs);
            return JsonNetResult.Success();
        }        

        [HttpPost]
        public ActionResult UpdateCogsDetails(
                long? pwMasterVariantId, long? pwMasterProductId, CogsDto defaults, List<CogsDto> details)
        {
            defaults.ValidateCurrency(_currencyService);
            details = details ?? new List<CogsDto>();             
            details.ForEach(x => x.ValidateCurrency(_currencyService));

            var userIdentity = HttpContext.PullIdentity();
            var service = _factory.MakeCogsService(userIdentity.PwShop);
            service.UpdateCogsWithDetails(pwMasterVariantId, pwMasterProductId, defaults, details);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult UpdateCogsForPickList(long pickListId, CogsDto simpleCogs)
        {
            simpleCogs.ValidateCurrency(_currencyService);

            var userIdentity = HttpContext.PullIdentity();
            var service = _factory.MakeCogsService(userIdentity.PwShop);
            service.UpdateCogsForPickList(pickListId, simpleCogs);
            return JsonNetResult.Success();
        }
    }
}

