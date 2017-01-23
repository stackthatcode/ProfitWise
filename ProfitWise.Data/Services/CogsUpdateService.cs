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

    
        public CogsUpdateServiceContext MakeUpdateContext(
                long? masterVariantId, PwCogsDetail defaults, IList<PwCogsDetail> details)
        {
            var defaultsWithConstraints =
                defaults
                    .CloneWithConstraints(ApplyConstraintsToDetail)
                    .AttachKeys(this.PwShop.PwShopId, masterVariantId);
            
            var detailsWithConstraints =
                    details?.Select(x => x.CloneWithConstraints(ApplyConstraintsToDetail)
                                        .AttachKeys(this.PwShop.PwShopId, masterVariantId))                                        
                            .ToList();

            var context = new CogsUpdateServiceContext
            {
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
                        });
                }

                return output;
            }
        }


        public void UpdateCogsForMasterVariant(
                    long? masterVariantId, PwCogsDetail defaults, IList<PwCogsDetail> details)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);

            using (var transaction = cogsEntryRepository.InitiateTransaction())
            {
                // Write the CoGS Entries
                var context = MakeUpdateContext(masterVariantId, defaults, details);
                UpdateCogsForMasterVariant(context);

                // Update the Report Entries
                cogsUpdateRepository.RefreshReportEntryData();

                transaction.Commit();
            }
        }

        public void UpdateCogsForMasterProduct(
                    long? masterProductId, PwCogsDetail defaults, IList<PwCogsDetail> details)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);

            var masterVariants = cogsEntryRepository.RetrieveVariants(new[] { masterProductId.Value });

            using (var transaction = cogsEntryRepository.InitiateTransaction())
            {
                foreach (var masterVariantId in masterVariants.Select(x => x.PwMasterVariantId))
                {
                    // Write the CoGS Entries
                    var context = MakeUpdateContext(masterVariantId, defaults, details);
                    UpdateCogsForMasterVariant(context);
                }

                // Update the Report Entries
                cogsUpdateRepository.RefreshReportEntryData();

                transaction.Commit();
            }
        }

        private void UpdateCogsForMasterVariant(CogsUpdateServiceContext context)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);

            // Save the Master Variant CoGS Defaults
            cogsEntryRepository.UpdateDefaultCogs(context.Defaults, context.HasDetails);

            // Save the Detail Entries
            if (context.HasDetails)
            {
                cogsEntryRepository.DeleteCogsDetail(context.PwMasterVariantId);

                foreach (var detail in context.Details)
                {
                    var detailWithConstraints = detail.CloneWithConstraints(ApplyConstraintsToDetail);
                    detailWithConstraints.PwMasterVariantId = context.PwMasterVariantId.Value;
                    cogsEntryRepository.InsertCogsDetails(detailWithConstraints);
                }
            }

            // Update the Order Lines
            var orderUpdateContexts = MakeUpdateOrderContexts(context);
            foreach (var orderUpdateContext in orderUpdateContexts)
            {
                cogsUpdateRepository.UpdateOrderLineUnitCogs(orderUpdateContext);
            }
        }


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

