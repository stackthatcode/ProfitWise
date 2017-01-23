using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Shop;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.Services
{
    [Intercept(typeof(ShopRequired))]
    public class CogsUpdateService
    {
        private readonly IPushLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly CurrencyService _currencyService;

        public PwShop PwShop { get; set; }

        public CogsUpdateService(
                IPushLogger logger, MultitenantFactory multitenantFactory, CurrencyService currencyService)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
            _currencyService = currencyService;
        }

    
        // Context translation
        public CogsUpdateServiceContext MakeUpdateEntryContext(
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

            var context = new CogsUpdateServiceContext
            {
                PwMasterProductId = masterProductId,
                PwMasterVariantId = masterVariantId,
                Defaults = defaultsWithConstraints,
                Details = detailsWithConstraints,
            };

            return context;
        }

        public IList<CogsUpdateOrderContext> MakeUpdateOrderContexts(CogsUpdateServiceContext sourceContext)
        {
            if (!sourceContext.HasDetails)
            {
                return new List<CogsUpdateOrderContext> {
                        new CogsUpdateOrderContext
                        {
                            Cogs = sourceContext.Defaults,
                            DestinationCurrencyId = this.PwShop.CurrencyId,
                            StartDate = null,
                            EndDate = null,
                            PwMasterProductId = sourceContext.PwMasterProductId,
                        }
                    };
            }
            else
            {
                var output = new List<CogsUpdateOrderContext>()
                {
                    new CogsUpdateOrderContext
                    {
                        Cogs = sourceContext.Defaults,
                        DestinationCurrencyId = this.PwShop.CurrencyId,
                        StartDate = null,
                        EndDate = sourceContext.FirstDetail.CogsDate.AddDays(-1),
                        PwMasterProductId = sourceContext.PwMasterProductId,
                    }
                };

                foreach (var detail in sourceContext.Details)
                {
                    var nextDetail = sourceContext.NextDetail(detail);
                    output.Add(
                        new CogsUpdateOrderContext
                        {
                            Cogs = detail,
                            DestinationCurrencyId = this.PwShop.CurrencyId,
                            StartDate = detail.CogsDate,
                            EndDate = nextDetail?.CogsDate.AddDays(-1),
                            PwMasterProductId = sourceContext.PwMasterProductId,
                        });
                }

                return output;
            }
        }


        // Update methods
        public void UpdateCogsForPickList(long pickListId, PwCogsDetail cogs)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);            

            using (var transaction = cogsEntryRepository.InitiateTransaction())
            {
                // Update Pick List Default Cogs
                cogsEntryRepository.UpdatePickListDefaultCogs(pickListId, cogs);

                // Update the Order Lines for the Pick List
                var context = new CogsUpdateOrderContextPickList()
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
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);

            using (var transaction = cogsEntryRepository.InitiateTransaction())
            {
                var context = MakeUpdateEntryContext(masterVariantId, null, defaults, details);

                // Write the CoGS Entries
                UpdateCogsEntryByContext(context);

                // Update the Order Lines for each division of Detail
                UpdateOrderLines(context);

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
                foreach (var masterVariantId in masterVariants.Select(x => x.PwMasterVariantId))
                {
                    // Write the CoGS Entries
                    var context = MakeUpdateEntryContext(masterVariantId, masterProductId, defaults, details);
                    UpdateCogsEntryByContext(context);
                }

                // Update the Order Lines for each division of Detail
                var orderLineContext = MakeUpdateEntryContext(null, masterProductId, defaults, details);
                UpdateOrderLines(orderLineContext);

                // Update the Report Entries
                cogsUpdateRepository.RefreshReportEntryData();

                transaction.Commit();
            }
        }

        private void UpdateCogsEntryByContext(CogsUpdateServiceContext context)
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

        private void UpdateOrderLines(CogsUpdateServiceContext context)
        {
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);
            var orderUpdateContexts = MakeUpdateOrderContexts(context);

            foreach (var orderUpdateContext in orderUpdateContexts)
            {
                cogsUpdateRepository.UpdateOrderLines(orderUpdateContext);
            }
        }


        // Constraints
        public void ApplyConstraintsToDetail(PwCogsDetail detail)
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

        public void ValidateCurrency(int cogsTypeId, int? cogsCurrencyId)
        {
            if (cogsTypeId != CogsType.FixedAmount) return;

            if (!cogsCurrencyId.HasValue || !_currencyService.CurrencyExists(cogsCurrencyId.Value))
            {
                throw new Exception($"Unable to locate Currency {cogsCurrencyId}");
            }
        }

        public decimal? ConstrainPercentage(decimal? cogsMarginPercent)
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
            if (cogsAmount > 999999999.99m)
            {
                return 999999999.99m;
            }
            return cogsAmount;
        }

    }
}

