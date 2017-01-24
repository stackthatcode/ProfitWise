using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Model.ShopifyImport;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.Services
{
    [Intercept(typeof(ShopRequired))]
    public class CogsService
    {
        private readonly IPushLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly CurrencyService _currencyService;


        public PwShop PwShop { get; set; }

        public CogsService(
                IPushLogger logger, MultitenantFactory multitenantFactory, CurrencyService currencyService)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
            _currencyService = currencyService;
        }

    
        // Translates Cogs Defaults and Details to uniform representation for saving Cogs data entry to database
        public CogsDataEntryUpdateContext MakeDataEntryUpdateContext(
                long? masterVariantId, long? masterProductId, PwCogsDetail defaults, IList<PwCogsDetail> details)
        {
            var defaultsWithConstraints =
                defaults
                    .CloneWithConstraints(ApplyConstraintsToDetail)
                    .AttachKeys(this.PwShop.PwShopId, masterVariantId, masterProductId);
            
            var detailsWithConstraints =
                details?.Select(x => x.CloneWithConstraints(ApplyConstraintsToDetail)
                                    .AttachKeys(this.PwShop.PwShopId, masterVariantId, masterProductId))                                        
                        .ToList();

            var context = new CogsDataEntryUpdateContext
            {
                PwMasterProductId = masterProductId,
                PwMasterVariantId = masterVariantId,
                Defaults = defaultsWithConstraints,
                Details = detailsWithConstraints,
            };

            return context;
        }

        // Translates Defaults and Details into a flattened sequence that can be processed as a list
        public IList<OrderLineCogsContext> MakeOrderLineUpdateContexts(PwMasterVariant masterVariant)
        {
            var defaults = new PwCogsDetail()
            {
                CogsTypeId = masterVariant.CogsTypeId,
                CogsAmount = masterVariant.CogsAmount,
                CogsCurrencyId = masterVariant.CogsCurrencyId,
                CogsMarginPercent = masterVariant.CogsMarginPercent,
            };

            // We're only creating this to leverage the same function for creating OrderLineUpdateContexts
            var entryContext =
                MakeDataEntryUpdateContext(
                    masterVariant.PwMasterVariantId, masterVariant.PwMasterProductId, defaults, masterVariant.CogsDetails);

            return MakeOrderLineUpdateContexts(entryContext);
        }

        public IList<OrderLineCogsContext> MakeOrderLineUpdateContexts(CogsDataEntryUpdateContext sourceContext)
        {
            if (!sourceContext.HasDetails)
            {
                return new List<OrderLineCogsContext> {
                        new OrderLineCogsContext
                        {
                            Cogs = sourceContext.Defaults,
                            DestinationCurrencyId = this.PwShop.CurrencyId,
                            StartDate = DateTime.MinValue,
                            EndDate = DateTime.MaxValue,
                            PwMasterProductId = sourceContext.PwMasterProductId,
                        }
                    };
            }
            else
            {
                var output = new List<OrderLineCogsContext>()
                {
                    new OrderLineCogsContext
                    {
                        Cogs = sourceContext.Defaults,
                        DestinationCurrencyId = this.PwShop.CurrencyId,
                        StartDate = DateTime.MinValue,
                        EndDate = sourceContext.FirstDetail.CogsDate.AddDays(-1),
                        PwMasterProductId = sourceContext.PwMasterProductId,
                    }
                };

                foreach (var detail in sourceContext.Details)
                {
                    var nextDetail = sourceContext.NextDetail(detail);
                    output.Add(new OrderLineCogsContext
                                {
                                    Cogs = detail,
                                    DestinationCurrencyId = this.PwShop.CurrencyId,
                                    StartDate = detail.CogsDate,
                                    EndDate = nextDetail?.CogsDate.AddDays(-1) ?? DateTime.MaxValue,
                                    PwMasterProductId = sourceContext.PwMasterProductId,
                                });
                }

                return output;
            }
        }



        // Update interface methods
        public void UpdateCogsForPickList(long pickListId, PwCogsDetail cogs)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);            

            using (var transaction = cogsEntryRepository.InitiateTransaction())
            {
                // Update Pick List Default Cogs
                cogsEntryRepository.UpdatePickListDefaultCogs(pickListId, cogs);

                // Update the Order Lines for the Pick List
                var context = new OrderLineCogsContextPickList()
                { 
                    Cogs = cogs.CloneWithConstraints(ApplyConstraintsToDetail),
                    PwPickListId = pickListId,
                    DestinationCurrencyId = this.PwShop.CurrencyId,                   
                };
                if (context.CogsTypeId == CogsType.FixedAmount)
                {
                    cogsUpdateRepository.UpdateOrderLineFixedAmount(context);
                }
                else
                {
                    cogsUpdateRepository.UpdateOrderLinePercentage(context);
                }

                // Update the Report Entries
                cogsUpdateRepository.RefreshReportEntryData();

                transaction.Commit();
            }
        }

        public void UpdateCogsForMasterVariant(
                    long? masterVariantId, PwCogsDetail defaults, IList<PwCogsDetail> details)
        {
            if (!masterVariantId.HasValue)
            {
                throw new ArgumentNullException("masterVariantId");
            }
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);

            using (var transaction = cogsEntryRepository.InitiateTransaction())
            {
                // Write the CoGS Entries
                var dataEntryContext = MakeDataEntryUpdateContext(masterVariantId, null, defaults, details);
                UpdateCogsEntryByContext(dataEntryContext);

                // Update the Order Lines for each division of Detail
                var orderLineContexts = MakeOrderLineUpdateContexts(dataEntryContext);
                foreach (var orderLineContext in orderLineContexts)
                {
                    UpdateOrderLines(orderLineContext);
                }

                // Update the Report Entries
                cogsUpdateRepository.RefreshReportEntryData();

                transaction.Commit();
            }
        }

        public void UpdateCogsForMasterProduct(
                    long? masterProductId, PwCogsDetail defaults, IList<PwCogsDetail> details)
        {
            if (!masterProductId.HasValue)
            {
                throw new ArgumentNullException("masterProductId");
            }
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);

            var masterVariants = cogsEntryRepository.RetrieveVariants(new[] { masterProductId.Value });
            using (var transaction = cogsEntryRepository.InitiateTransaction())
            {
                // First we'll save the CoGS data entry for all child Master Variants
                foreach (var masterVariantId in masterVariants.Select(x => x.PwMasterVariantId))
                {
                    // Write the CoGS Entries
                    var masterVariantContext = MakeDataEntryUpdateContext(masterVariantId, masterProductId, defaults, details);
                    UpdateCogsEntryByContext(masterVariantContext);
                }

                // Next, create a data entry context (for Master Product)...
                var masterProductContext = MakeDataEntryUpdateContext(null, masterProductId, defaults, details);

                // ... to translate into Order Line update contexts
                var orderLineContexts = MakeOrderLineUpdateContexts(masterProductContext);
                foreach (var orderLineContext in orderLineContexts)
                {
                    UpdateOrderLines(orderLineContext);
                }

                // Update the Report Entries
                cogsUpdateRepository.RefreshReportEntryData();
                transaction.Commit();
            }
        }

        // Update worker methods
        private void UpdateCogsEntryByContext(CogsDataEntryUpdateContext context)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            
            // Save the Master Variant CoGS Defaults
            cogsEntryRepository.UpdateMasterVariantDefaultCogs(context.Defaults, context.HasDetails);

            // IF they removed all Detail, this ensures everything is clear...
            cogsEntryRepository.DeleteCogsDetail(context.PwMasterVariantId);

            // Save the Detail Entries
            if (context.HasDetails)
            {
                foreach (var detail in context.Details)
                {
                    var detailWithConstraints = detail.CloneWithConstraints(ApplyConstraintsToDetail);
                    detailWithConstraints.PwMasterVariantId = context.PwMasterVariantId.Value;
                    cogsEntryRepository.InsertCogsDetails(detailWithConstraints);
                }
            }
        }

        private void UpdateOrderLines(OrderLineCogsContext context)
        {
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);
            cogsUpdateRepository.UpdateOrderLines(context);
        }


        // In-memory computation for the Order Refresh Step
        public decimal CalculateUnitCogs(IList<OrderLineCogsContext> contexts, ShopifyOrderLineItem lineItem)
        {
            var context = contexts.SelectContextByDate(lineItem.OrderDate);
            if (context == null)
            {
                throw new Exception($"Unable to locate OrderLineCogsContext for Order Line {lineItem.ShopifyOrderLineId}");
            }

            if (context.CogsTypeId == CogsType.FixedAmount)
            {
                if (!context.CogsAmount.HasValue)
                    throw new Exception("Missing CogsAmount");
                if (!context.CogsCurrencyId.HasValue)
                    throw new Exception("Missing CogsCurrencyId");

                return _currencyService.Convert(
                        context.CogsAmount.Value, context.CogsCurrencyId.Value,
                        context.DestinationCurrencyId, lineItem.OrderDate);
            }
            else
            {
                return lineItem.UnitPrice * context.CogsPercentOfUnitPrice;
            }
        }



        // Constraints
        private void ApplyConstraintsToDetail(PwCogsDetail detail)
        {
            ValidateCurrency(detail.CogsTypeId, detail.CogsCurrencyId);
            detail.CogsAmount = ConstrainAmount(detail.CogsAmount);
            detail.CogsMarginPercent = ConstrainPercentage(detail.CogsMarginPercent);
            if (detail.CogsTypeId == CogsType.FixedAmount)
            {
                detail.CogsMarginPercent = null;
            }
            if (detail.CogsTypeId == CogsType.MarginPercentage)
            {
                detail.CogsCurrencyId = null;
                detail.CogsAmount = null;
            }
        }

        private void ValidateCurrency(int cogsTypeId, int? cogsCurrencyId)
        {
            if (cogsTypeId != CogsType.FixedAmount) return;

            if (!cogsCurrencyId.HasValue || !_currencyService.CurrencyExists(cogsCurrencyId.Value))
            {
                throw new Exception($"Unable to locate Currency {cogsCurrencyId}");
            }
        }

        private decimal? ConstrainPercentage(decimal? cogsMarginPercent)
        {
            if (!cogsMarginPercent.HasValue)
            {
                return cogsMarginPercent;
            }
            if (cogsMarginPercent < 0m)
            {
                return 0m;
            }
            if (cogsMarginPercent > 100m)
            {
                return 100m;
            }
            return cogsMarginPercent;
        }

        private decimal? ConstrainAmount(decimal? cogsAmount)
        {
            if (!cogsAmount.HasValue)
            {
                return cogsAmount;
            }
            if (cogsAmount < 0m)
            {
                return 0m;
            }
            if (cogsAmount > 999999999.99m)
            {
                return 999999999.99m;
            }
            return cogsAmount;
        }

    }
}

