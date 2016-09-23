using System;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
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
            var userBrief = HttpContext.PullUserBriefFromContext();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.Shop);
            var queryId = cogsRepository.InsertQuery();

            var terms =
                parameters.Text
                    .Split(' ')
                    .Select(x => x.Trim())
                    .Where(x => x != "")
                    .ToList();

            cogsRepository.InsertMasterProductPickList(queryId, terms);

            var products = cogsRepository.RetrieveMasterProducts(queryId, 1, 50);
            var variants = 
                cogsRepository
                    .RetrieveMasterVariants(products.Select(x => x.PwMasterProductId).ToList());

            foreach (var variant in variants)
            {
                if (variant.CogsAmount != null && variant.CogsCurrencyId != null)
                {
                    variant.NormalizedCogsAmount = 
                        _currencyService.Convert(
                            variant.CogsAmount.Value, 
                            variant.CogsCurrencyId.Value, 
                            userBrief.Shop.CurrencyId, 
                            DateTime.Now);
                }
            }

            products.PopulateVariants(variants);

            return new JsonNetResult(new { Shop = userBrief.Shop, Products = products });
        }

    }
}

