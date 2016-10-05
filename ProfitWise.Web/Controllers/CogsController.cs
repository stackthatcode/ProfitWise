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
    public class CogsController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;

        public CogsController(MultitenantFactory factory, CurrencyService currencyService)
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

            var model = products.ToCogsGridModel(userBrief.PwShop.CurrencyId);
            return new JsonNetResult(new { products = model, totalRecords = recordCount });

        }

        [HttpPost]
        public ActionResult BulkUpdateCogs(long masterProductId, int currencyId, decimal amount)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            cogsRepository.UpdateProductCogsAllVariants(masterProductId, currencyId, amount);

            return new JsonNetResult(new { Success = true});
        }



    }
}

