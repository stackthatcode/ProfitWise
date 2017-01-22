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

        public CogsUpdateContext MakeUpdateContext(
                long? masterVariantId, long? masterProductId, PwCogsDetail defaults, IList<PwCogsDetail> details)
        {

            var defaultsWithConstraints = defaults.CloneWithConstraints(ApplyConstraintsToDetail);
            var detailsWithConstraints =
                    details?.Select(x => x.CloneWithConstraints(ApplyConstraintsToDetail)).ToList();

            var context = new CogsUpdateContext
            {
                PwMasterVariantId = masterVariantId,                
                Defaults = defaultsWithConstraints,
                Details = detailsWithConstraints,
            };

            return context;
        }


        public void UpdateCogsForMasterVariant(CogsUpdateContext context)
        {
            var cogsRepository = _multitenantFactory.MakeCogsRepository(PwShop);

            using (var transaction = cogsRepository.InitiateTransaction())
            {
                // Save the Master Variant CoGS Defaults
                cogsRepository.UpdateDefaultCogs(context.Defaults, context.HasDetails);

                // Save the Detail Entries
                if (context.HasDetails)
                {
                    cogsRepository.DeleteCogsDetail(context.PwMasterVariantId);

                    foreach (var detail in context.Details)
                    {
                        var detailWithConstraints = detail.CloneWithConstraints(ApplyConstraintsToDetail);
                        detailWithConstraints.PwMasterVariantId = context.PwMasterVariantId.Value;
                        cogsRepository.InsertCogsDetails(detailWithConstraints);
                    }
                }

                // Update the Order Lines



                // Update the Report Entries


                transaction.Commit();
            }
        }



        public void ApplyConstraintsToDetail(PwCogsDetail detail)
        {
            ValidateCurrency(detail.CogsTypeId, detail.CogsCurrencyId);
            detail.CogsAmount = ConstrainAmount(detail.CogsAmount);
            detail.CogsPercentage = ConstrainPercentage(detail.CogsPercentage);
            if (detail.CogsTypeId == CogsType.FixedAmount)
            {
                detail.CogsPercentage = null;
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

        public decimal? ConstrainPercentage(decimal? cogsPercentage)
        {
            if (!cogsPercentage.HasValue)
            {
                return cogsPercentage;
            }
            if (cogsPercentage < 0m)
            {
                return 0m;
            }
            if (cogsPercentage > 100m)
            {
                return 100m;
            }
            return cogsPercentage;
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

