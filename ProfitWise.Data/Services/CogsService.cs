using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
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
        private readonly ConnectionWrapper _connectionWrapper;

        public PwShop PwShop { get; set; }

        public CogsService(
                IPushLogger logger, 
                MultitenantFactory multitenantFactory, 
                CurrencyService currencyService,
                ConnectionWrapper connectionWrapper)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
            _currencyService = currencyService;
            _connectionWrapper = connectionWrapper;
        }

        public void UpdateSimpleCogs(long pwMasterVariantId, CogsDto simpleCogs)
        {
            using (var transaction = _connectionWrapper.StartTransactionForScope())
            {
                var context = MasterVariantUpdateContext.Make(pwMasterVariantId, simpleCogs, null);
                UpdateCogsForMasterVariant(context);

                var orderLineContexts = 
                    OrderLineUpdateContext.Make(
                        simpleCogs, null, PwShop.CurrencyId, null, pwMasterVariantId);
                UpdateOrderLinesAndEntryData(orderLineContexts);

                transaction.Commit();
            }
        }

        public void UpdateCogsDetails(
                long? pwMasterVariantId, long? pwMasterProductId, CogsDto defaults, List<CogsDto> details)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);

            if (pwMasterVariantId.HasValue)
            {
                using (var transaction = _connectionWrapper.StartTransactionForScope())
                {
                    var context = MasterVariantUpdateContext.Make(pwMasterVariantId, defaults, details);
                    UpdateCogsForMasterVariant(context);

                    var orderLineContexts = 
                        OrderLineUpdateContext.Make(
                            defaults, details, PwShop.CurrencyId, null, pwMasterVariantId);
                    UpdateOrderLinesAndEntryData(orderLineContexts);

                    transaction.Commit();
                    return;
                }
            }

            if (pwMasterProductId.HasValue)
            {
                using (var transaction = _connectionWrapper.StartTransactionForScope())
                {
                    var masterVariants = cogsEntryRepository.RetrieveVariants(new[] {pwMasterProductId.Value});
                    foreach (var masterVariantId in masterVariants.Select(x => x.PwMasterVariantId))
                    {
                        var context = MasterVariantUpdateContext.Make(masterVariantId, defaults, details);
                        UpdateCogsForMasterVariant(context);
                    }

                    var orderLineContexts = 
                        OrderLineUpdateContext.Make(
                            defaults, details, PwShop.CurrencyId, pwMasterProductId, null);

                    UpdateOrderLinesAndEntryData(orderLineContexts);

                    transaction.Commit();
                    return;
                }
            }
        }

        public void UpdateCogsForMasterVariant(MasterVariantUpdateContext context)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);

            // Save the Master Variant CoGS Defaults
            cogsEntryRepository.UpdateMasterVariantDefaultCogs(context.Defaults, context.HasDetails);

            // If they removed all Detail, this ensures everything is clear...
            cogsEntryRepository.DeleteCogsDetail(context.PwMasterVariantId);

            // Save the Detail Entries
            if (context.HasDetails)
            {
                foreach (var detail in context.Details)
                {
                    cogsEntryRepository.InsertCogsDetails(detail);
                }
            }
        }

        public void UpdateCogsForPickList(long pickListId, CogsDto cogs)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);

            using (var trans = _connectionWrapper.StartTransactionForScope())
            {
                // Update Pick List Default Cogs
                cogsEntryRepository.UpdatePickListDefaultCogs(pickListId, cogs);

                // Update the Order Lines for the Pick List
                var context = OrderLineUpdateContext.Make(cogs, this.PwShop.CurrencyId, pickListId);
                if (context.CogsTypeId == CogsType.FixedAmount)
                {
                    cogsUpdateRepository.UpdateOrderLineFixedAmountPickList(context);
                }
                else
                {
                    cogsUpdateRepository.UpdateOrderLinePercentagePickList(context);
                }

                // Update the Report Entries
                cogsUpdateRepository.RefreshReportEntryData();
                trans.Commit();
            }
        }

        public void UpdateOrderLinesAndEntryData(IList<OrderLineUpdateContext> orderLineContexts)
        {
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDataUpdateRepository(PwShop);
            
            foreach (var orderLineContext in orderLineContexts)
            {
                cogsUpdateRepository.UpdateOrderLines(orderLineContext);
            }

            cogsUpdateRepository.RefreshReportEntryData();
        }
        

        // In-memory computation for the Order Refresh Step
        public decimal CalculateUnitCogs(
                IList<OrderLineUpdateContext> contexts, ShopifyOrderLineItem lineItem)
        {
            var context = contexts.SelectContextByDate(lineItem.OrderDate);
            if (context == null)
            {
                throw new Exception(
                    $"Unable to locate OrderLineCogsContext for Order Line {lineItem.ShopifyOrderLineId}");
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

        // Not sure this belongs her?
        public void ValidateCurrency(int cogsTypeId, int? cogsCurrencyId)
        {
            if (cogsTypeId != CogsType.FixedAmount) return;

            if (!cogsCurrencyId.HasValue || !_currencyService.CurrencyExists(cogsCurrencyId.Value))
            {
                throw new Exception($"Unable to locate Currency {cogsCurrencyId}");
            }
        }
    }
}

