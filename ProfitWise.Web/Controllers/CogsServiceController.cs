﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Catalog;
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

        public CogsServiceController(
                MultitenantFactory factory, 
                CurrencyService currencyService)
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
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);
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

        [HttpPost]
        public ActionResult BulkUpdateCogs(long masterProductId, int currencyId, decimal amount)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);
            
            // TODO => Revisit
            //ValidateCogsByAmounts(currencyId, amount);

            cogsRepository.UpdateProductCogsAllVariants(masterProductId, currencyId, amount);
            return JsonNetResult.Success();
        }



        [HttpPost]
        public ActionResult StockedDirectlyByPickList(long pickListId, bool newValue)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            cogsRepository.UpdateStockedDirectlyByPicklist(pickListId, newValue);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult StockedDirectlyByMasterProductId(long masterProductId, bool value)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            cogsRepository.UpdateStockedDirectlyByMasterProductId(masterProductId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult ExcludeByPickList(long pickListId, bool value)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            cogsRepository.UpdateExcludeByPicklist(pickListId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult ExcludeByMasterProductId(long masterProductId, bool value)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            cogsRepository.UpdateExcludeByMasterProductId(masterProductId, value);
            return JsonNetResult.Success();
        }


        // Returns PwCogsProduct
        [HttpGet]
        public ActionResult RetrieveMasterProduct(long masterProductId)
        {
            var userIdentity = HttpContext.PullIdentity();
            var shopCurrencyId = userIdentity.PwShop.CurrencyId;
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            var masterProductSummary = cogsRepository.RetrieveProduct(masterProductId);
            if (masterProductSummary == null)
            {
                return new JsonNetResult(new { MasterProduct = (CogsMasterProductModel)null });
            }

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
        public ActionResult UpdateCogs(
                long masterVariantId, int cogsTypeId, int? cogsCurrencyId, decimal? cogsAmount, decimal? cogsPercentage)
        {
            ValidateCurrency(cogsTypeId, cogsCurrencyId);
            cogsAmount = ConstrainAmount(cogsAmount);
            cogsPercentage = ConstrainPercentage(cogsPercentage);

            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            cogsRepository.UpdateDefaultCogs(
                masterVariantId, cogsTypeId, cogsCurrencyId, cogsAmount, cogsPercentage, false);

            // cogsRepository.UpdateOrderLinesWithSimpleCogs(masterVariantId);

            return JsonNetResult.Success();
        }

        public void ValidateCurrency(int cogsTypeId, int? cogsCurrencyId)
        {
            if (cogsTypeId != CogsType.FixedAmount) return;

            if (!cogsCurrencyId.HasValue || !_currencyService.CurrencyExists(cogsCurrencyId.Value))
            {
                throw new Exception($"Unable to locate Currency {cogsCurrencyId}");
            }
        }

        public decimal? ConstrainPercentage(decimal? cogsPercentage)
        {
            if (!cogsPercentage.HasValue)
            {
                return cogsPercentage;
            }
            if (cogsPercentage < 0m)
            {
                return 0m;
            }
            if (cogsPercentage > 100m)
            {
                return 100m;
            }
            return cogsPercentage;
        }

        public decimal? ConstrainAmount(decimal? cogsAmount)
        {
            if (!cogsAmount.HasValue)
            {
                return cogsAmount;
            }
            if (cogsAmount < 0m)
            {
                return 0m;
            }
            if (cogsAmount > 999999999m)
            {
                return 100m;
            }
            return cogsAmount;
        }

        [HttpPost]
        public ActionResult CogsDetails(
                long? masterVariantId, long? masterProductId, 
                PwCogsDetail defaults, List<PwCogsDetail> details)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            ValidateCurrency(defaults.CogsTypeId, defaults.CogsCurrencyId);

            using (var transaction = cogsRepository.InitiateTransaction())
            {
                var hasDetails = details != null && details.Any();

                cogsRepository.UpdateDefaultCogs(
                    masterVariantId,
                    defaults.CogsTypeId,
                    defaults.CogsCurrencyId,
                    ConstrainAmount(defaults.CogsAmount),
                    ConstrainPercentage(defaults.CogsPercentage),
                    hasDetails);

                cogsRepository.DeleteCogsDetail(masterVariantId);
                
                if (hasDetails)
                {
                    foreach (var detail in details)
                    {
                        detail.PwMasterVariantId = masterVariantId.Value;
                        cogsRepository.InsertCogsDetails(detail);
                    }
                }

                transaction.Commit();
            }

            return JsonNetResult.Success();
        }


        [HttpPost]
        public ActionResult ExcludeByMasterVariantId(long masterVariantId, bool value)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            cogsRepository.UpdateExcludeByMasterVariantId(masterVariantId, value);
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult StockedDirectlyByMasterVariantId(long masterVariantId, bool value)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsRepository(userIdentity.PwShop);

            cogsRepository.UpdateStockedDirectlyByMasterVariantId(masterVariantId, value);
            return JsonNetResult.Success();
        }
    }
}

