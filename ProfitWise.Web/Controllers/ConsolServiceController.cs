using System.Linq;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using ProfitWise.Data.Factories;
using ProfitWise.Web.Attributes;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Web.Json;


namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    [IdentityProcessor]
    [MaintenanceAttribute]
    public class ConsolServiceController : Controller
    {
        private readonly MultitenantFactory _factory;

        public ConsolServiceController(MultitenantFactory factory)
        {
            _factory = factory;
        }

        [HttpPost]
        public ActionResult Search(long pwMasterProductId, long? pickListId, string terms)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
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
                pickListRepository.Filter(newPickListId, pwMasterProductId);
                trans.Commit();
            }

            return new JsonNetResult(new { PickListId = newPickListId });
        }

        [HttpPost]
        public ActionResult RetrieveResults(
                long pickListId, int pageNumber, int pageSize, int sortByColumn, bool sortByDirectionDown)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var cogsRepository = _factory.MakeCogsEntryRepository(userIdentity.PwShop);
            var pickListRepository = _factory.MakePickListRepository(userIdentity.PwShop);

            if (!pickListRepository.Exists(pickListId))
            {
                return new JsonNetResult(new {pickListValid = false,});
            }

            var recordCount = pickListRepository.Count(pickListId);
            var products = cogsRepository
                .RetrieveCogsSummaryFromPicklist(pickListId, pageNumber, pageSize, sortByColumn, sortByDirectionDown);

            return new JsonNetResult(new { pickListValid = true, products, totalRecords = recordCount });
        }


        [HttpGet]
        public ActionResult MasterProduct(long pwMasterProductId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var productRepository = _factory.MakeProductRepository(userIdentity.PwShop);
            var variantRepository = _factory.MakeVariantRepository(userIdentity.PwShop);

            var product = productRepository.RetrieveProducts(pwMasterProductId);
            var variants = variantRepository.RetrieveVariantsForMasterProduct(pwMasterProductId);

            return new JsonNetResult(new { product, variants });
        }        

        [HttpGet]
        public ActionResult MasterVariant(long pwMasterVariantId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var variantRepository = _factory.MakeVariantRepository(userIdentity.PwShop);
            var variants = variantRepository.RetrieveVariantsForMasterVariant(pwMasterVariantId);

            return new JsonNetResult(new { variants });
        }

        [HttpPost]
        public ActionResult ConsolidateProduct(long targetMasterProductId, long inboundMasterProductId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var service = _factory.MakeConsolidationService(userIdentity.PwShop);
            var repository = _factory.MakeProductRepository(userIdentity.PwShop);

            using (var transaction = service.InitiateTransaction())
            {
                var productIds = repository.RetrieveAllChildProductIds(inboundMasterProductId);
                foreach (var inboundProductId in productIds)
                {
                    service.ConsolidateProduct(inboundProductId, targetMasterProductId);
                }
                transaction.Commit();
            }
            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult DeconsolidateProduct(long pwProductId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var service = _factory.MakeConsolidationService(userIdentity.PwShop);

            using (var transaction = service.InitiateTransaction())
            {
                service.DeconsolidateProduct(pwProductId);
                transaction.Commit();
            }
            return JsonNetResult.Success();

        }


        [HttpPost]
        public ActionResult PrimaryProduct(long pwMasterProductId, long pwProductId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var builderService = _factory.MakeCatalogBuilderService(userIdentity.PwShop);
            var retrievalService = _factory.MakeCatalogRetrievalService(userIdentity.PwShop);
            var repository = _factory.MakeProductRepository(userIdentity.PwShop);
            
            using (var transaction = builderService.InitiateTransaction())
            {
                var masterProduct = retrievalService.RetrieveMasterProduct(pwMasterProductId);
                if (masterProduct == null)
                {
                    return JsonNetResult.Success();
                }
                var product = masterProduct.Product(pwProductId);
                if (product == null)
                {
                    return JsonNetResult.Success();
                }

                masterProduct.PrimaryManual(product);
                masterProduct.Products.ForEach(x => repository.UpdateProductIsPrimary(x));
                
                transaction.Commit();
            }
            return JsonNetResult.Success();
        }


        [HttpPost]
        public ActionResult ConsolidateVariant(long targetMasterVariantId, long inboundMasterVariantId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var service = _factory.MakeConsolidationService(userIdentity.PwShop);

            using (var transaction = service.InitiateTransaction())
            {
                service.ConsolidateVariant(inboundMasterVariantId, targetMasterVariantId);
                transaction.Commit();
            }

            return JsonNetResult.Success();
        }

        [HttpPost]
        public ActionResult DeconsolidateVariant(long pwVariantId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var service = _factory.MakeConsolidationService(userIdentity.PwShop);

            using (var transaction = service.InitiateTransaction())
            {
                service.DeconsolidateVariant(pwVariantId);
                transaction.Commit();
            }

            return JsonNetResult.Success();
        }


        [HttpPost]
        public ActionResult PrimaryVariant(long pwMasterVariantId, long pwVariantId)
        {
            var userIdentity = HttpContext.IdentitySnapshot();
            var builderService = _factory.MakeCatalogBuilderService(userIdentity.PwShop);
            var repository = _factory.MakeVariantRepository(userIdentity.PwShop);

            using (var transaction = builderService.InitiateTransaction())
            {
                var masterVariant =
                    repository.RetrieveMasterVariants(pwMasterVariantId: pwMasterVariantId)
                        .FirstOrDefault();
                if (masterVariant == null)
                {
                    return JsonNetResult.Success();
                }
                var variant = masterVariant.Variant(pwVariantId);
                if (variant == null)
                {
                    return JsonNetResult.Success();
                }

                masterVariant.PrimaryManual(variant);
                masterVariant.Variants.ForEach(x => repository.UpdateVariantIsPrimary(x));

                transaction.Commit();
            }
            return JsonNetResult.Success();
        }

    }
}

