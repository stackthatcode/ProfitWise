using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    public class ReportServiceController : Controller
    {
        private readonly MultitenantFactory _factory;

        public ReportServiceController(MultitenantFactory factory, CurrencyService currencyService)
        {
            _factory = factory;
        }


    }
}
