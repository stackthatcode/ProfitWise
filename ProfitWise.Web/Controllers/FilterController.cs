using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class FilterController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;

        public FilterController(MultitenantFactory factory, CurrencyService currencyService)
        {
            _factory = factory;
            _currencyService = currencyService;
        }


        public ActionResult Editor(long filterId)
        {
            var model = new FilterEditorModel()
            {
                FilterId = filterId,
            };

            return View(model);
        }
        
        
        [HttpGet]
        public ActionResult Ping()
        {
            return new JsonNetResult(new { Success = true });
        }


        private PwCogsProduct RetrieveProduct(int masterProductId)
        {
            var userBrief = HttpContext.PullIdentitySnapshot();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.PwShop);

            var product = cogsRepository.RetrieveProduct(masterProductId);
            product.Variants = cogsRepository.RetrieveVariants(new List<long> { masterProductId });
            product.PopulateNormalizedCogsAmount(_currencyService, userBrief.PwShop.CurrencyId);

            return product;
        }
    }
}

