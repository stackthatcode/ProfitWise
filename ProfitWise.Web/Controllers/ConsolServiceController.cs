using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
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
    public class ConsolServiceController : Controller
    {
        private readonly MultitenantFactory _factory;

        public ConsolServiceController(MultitenantFactory factory)
        {
            _factory = factory;
        }

        [HttpPost]
        public ActionResult Search(long? pickListId, string terms)
        {
            var userIdentity = HttpContext.PullIdentity();
            var pickListRepository = _factory.MakePickListRepository(userIdentity.PwShop);

            long newPickListId;

            using (var trans = pickListRepository.InitiateTransaction())
            {
                if (pickListId.HasValue)
                {
                    // TODO - this may be unnecessary; possibly leave it to a late night batch job
                    pickListRepository.Delete(pickListId.Value);
                }

                newPickListId = pickListRepository.CreateNew();

                var splitTerms = (terms ?? "").SplitBy(',');
                pickListRepository.Populate(newPickListId, splitTerms);
                
                trans.Commit();
            }

            return new JsonNetResult(new { PickListId = newPickListId });
        }

        [HttpPost]
        public ActionResult RetrieveResults(long pickListId, int pageNumber, int pageSize)
        {
            var userIdentity = HttpContext.PullIdentity();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);
            var pickListRepository = _factory.MakePickListRepository(userIdentity.PwShop);

            if (!pickListRepository.Exists(pickListId))
            {
                return new JsonNetResult(new {pickListValid = false,});
            }

            var recordCount = pickListRepository.Count(pickListId);
            var products = cogsRepository.RetrieveProductsFromPicklist(pickListId, pageNumber, pageSize);

            return new JsonNetResult(new { pickListValid = true, products, totalRecords = recordCount });
        }
    }
}

