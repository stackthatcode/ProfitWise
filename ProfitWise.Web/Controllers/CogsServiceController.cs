using System;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
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
            var userBrief = HttpContext.PullIdentitySnapshot();
            var pickListRepository = _factory.MakePickListRepository(userBrief.PwShop);

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
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);
            var pickListRepository = _factory.MakePickListRepository(userBrief.PwShop);
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

            products.PopulateNormalizedCogsAmount(_currencyService, userBrief.PwShop.CurrencyId);

            // Notice: we're using the Shop Currency to represent the price
            var model = products.ToCogsGridModel(userBrief.PwShop.CurrencyId);
            return new JsonNetResult(new { products = model, totalRecords = recordCount });
        }

        [HttpPost]
        public ActionResult BulkUpdateCogs(long masterProductId, int currencyId, decimal amount)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);
            
            ValidateCogs(currencyId, amount);

            cogsRepository.UpdateProductCogsAllVariants(masterProductId, currencyId, amount);
            return JsonNetResult.Success();
        }


        [HttpPost]
        public ActionResult StockedDirectlyByPickList(long pickListId, bool newValue)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            cogsRepository.UpdateStockedDirectlyByPicklist(pickListId, newValue);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult StockedDirectlyByMasterProductId(long masterProductId, bool value)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            cogsRepository.UpdateStockedDirectlyByMasterProductId(masterProductId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult ExcludeByPickList(long pickListId, bool value)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            cogsRepository.UpdateExcludeByPicklist(pickListId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult ExcludeByMasterProductId(long masterProductId, bool value)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            cogsRepository.UpdateExcludeByMasterProductId(masterProductId, value);
            return JsonNetResult.Success();
        }


        // Returns PwCogsProduct
        [HttpGet]
        public ActionResult RetrieveMasterProduct(long masterProductId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var shopCurrencyId = userBrief.PwShop.CurrencyId;
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            var masterProductSummary = cogsRepository.RetrieveProduct(masterProductId);
            var masterVariants = cogsRepository.RetrieveVariants(new[] { masterProductId });

            var masterProduct = new CogsMasterProductModel()
            {
                MasterProductId = masterProductSummary.PwMasterProductId,
                Title = masterProductSummary.Title,
            };

            masterProduct.MasterVariants = 
                masterVariants.Select(x => CogsMasterVariantModel.Build(x, shopCurrencyId)).ToList();

            return new JsonNetResult(new { MasterProduct = masterProduct });
        }

        [HttpPost]
        public ActionResult UpdateCogs(long masterVariantId, int currencyId, decimal amount)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);
            cogsRepository.UpdateMasterVariantCogs(masterVariantId, currencyId, amount);

            ValidateCogs(currencyId, amount);

            return JsonNetResult.Success();
        }

        public void ValidateCogs(int currencyId, decimal amount)
        {
            if (!_currencyService.CurrencyExists(currencyId))
            {
                throw new Exception($"Unable to locate Currency {currencyId} (Amount: {amount}");
            }
            if (amount < 0 || amount > 999999999m)
            {
                throw new Exception($"Dollar Amount {amount} out of acceptable range (Currency Id: {currencyId})");
            }
        }


        [HttpPost]
        public ActionResult ExcludeByMasterVariantId(long masterVariantId, bool value)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            cogsRepository.UpdateExcludeByMasterVariantId(masterVariantId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult StockedDirectlyByMasterVariantId(long masterVariantId, bool value)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            cogsRepository.UpdateStockedDirectlyByMasterVariantId(masterVariantId, value);
            return JsonNetResult.Success();
        }

    }
}

