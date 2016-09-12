using System.Web.Mvc;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    public class CogsController
    {

    }
}

