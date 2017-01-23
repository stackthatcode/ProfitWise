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

            long pickListId;

            using (var transaction = pickListRepository.InitiateTransaction())
            {
                pickListId = pickListRepository.Provision();

                var terms = (parameters.Text ?? "").SplitBy(',');
                pickListRepository.Populate(pickListId, terms);

                if (parameters.Filters != null && parameters.Filters.Count > 0)
                {
                    pickListRepository.Filter(pickListId, parameters.Filters);

                    if (parameters.Filters.Any(x => x.Type == ProductSearchFilterType.MissingCogs))
                    {
                        pickListRepository.FilterMissingCogs(pickListId);
                    }
                }

                transaction.Commit();
            }
            
            return new JsonNetResult(new { PickListId = pickListId});
        }

        [HttpPost]
        public ActionResult RetrieveResults(SearchResultSelection resultSelection)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);
            var pickListRepository = _factory.MakePickListRepository(userIdentity.PwShop);
            var recordCount = pickListRepository.Count(resultSelection.PickListId);

            // Pull the Search Results by Pick List page number
            var products =
                cogsRepository.RetrieveProductsFromPicklist(
                    resultSelection.PickListId,
                    resultSelection.PageNumber, 
                    resultSelection.PageSize,
                    resultSelection.SortByColumn, 
                    resultSelection.SortByDirectionDown);

            products.PopulateVariants(
                cogsRepository
                    .RetrieveVariants(products.Select(x => x.PwMasterProductId).ToList()));

            products.PopulateNormalizedCogsAmount(_currencyService, userIdentity.PwShop.CurrencyId);

            // Notice: we're using the Shop Currency to represent the price
            var model = products.ToCogsGridModel(userIdentity.PwShop.CurrencyId);
            return new JsonNetResult(new { products = model, totalRecords = recordCount });
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
                return new JsonNetResult(new { MasterProduct = (CogsMasterProductModel)null });
            }

            var masterVariants = cogsRepository.RetrieveVariants(new[] { pwMasterProductId });

            var masterProduct = new CogsMasterProductModel()
            {
                PwMasterProductId = masterProductSummary.PwMasterProductId,
                Title = masterProductSummary.Title,
            };
            masterProduct.MasterVariants =
                masterVariants.Select(x => CogsMasterVariantModel.Build(x, shopCurrencyId)).ToList();

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



        // TODO - revisit
        [HttpPost]
        public ActionResult BulkUpdateCogs(long pwMasterProductId, int currencyId, decimal amount)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);

            cogsRepository.UpdateProductCogsAllVariants(pwMasterProductId, currencyId, amount);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult UpdateSimpleCogs(PwCogsDetail simpleCogs)
        {
            var userIdentity = HttpContext.PullIdentity();
            var service = _factory.MakeCogsUpdateService(userIdentity.PwShop);
            service.UpdateCogsForMasterVariant(simpleCogs.PwMasterVariantId, simpleCogs, null);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult UpdateCogsDetails(
                long? pwMasterVariantId, long? pwMasterProductId, PwCogsDetail defaults, List<PwCogsDetail> details)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsService = _factory.MakeCogsUpdateService(userIdentity.PwShop);
            
            if (pwMasterVariantId.HasValue)
            {
                cogsService.UpdateCogsForMasterVariant(pwMasterVariantId, defaults, details);
                return JsonNetResult.Success();
            }
            if (pwMasterProductId.HasValue)
            {
                cogsService.UpdateCogsForMasterProduct(pwMasterProductId, defaults, details);
                return JsonNetResult.Success();
            }
            throw new ArgumentNullException("Both Master pwMasterVariantId and pwMasterProductId are null");
        }

    }
}

