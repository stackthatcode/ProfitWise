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
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            using (var transaction = cogsRepository.InitiateTransaction())
            {
                var terms = (parameters.Text ?? "").SplitBy(',');
                cogsRepository.InsertPickList(terms);

                if (parameters.Filters != null && parameters.Filters.Count > 0)
                {
                    cogsRepository.FilterPickList(parameters.Filters);
                    if (parameters.Filters.Any(x => x.Type == ProductSearchFilterType.MissingCogs))
                    {
                        cogsRepository.FilterPickListMissingCogs();
                    }
                }

                var products =
                    cogsRepository.RetrieveProductsFromPicklist(
                        parameters.PageNumber, parameters.PageSize, parameters.SortByColumn, parameters.SortByDirectionDown);

                products.PopulateVariants(
                    cogsRepository
                        .RetrieveVariants(products.Select(x => x.PwMasterProductId).ToList()));

                products.PopulateNormalizedCogsAmount(_currencyService, userBrief.PwShop.CurrencyId);
                

                var model = products.ToCogsGridModel(userBrief.PwShop.CurrencyId);
                var recordCount = cogsRepository.RetreivePickListCount();

                return new JsonNetResult(new {products = model, totalRecords = recordCount});
            }
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

