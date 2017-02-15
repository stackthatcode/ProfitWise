﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
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
                var dateBlockContexts =
                    CogsDateBlockContext.Make(
                        simpleCogs, null, PwShop.CurrencyId, null, pwMasterVariantId);

                UpdateCogsForMasterVariant(context);
                UpdateGoodsOnHandForMasterVariant(dateBlockContexts);
                UpdateOrderLinesAndReportEntries(dateBlockContexts);

                transaction.Commit();
            }
        }

        public void UpdateCogsWithDetails(
                long? pwMasterVariantId, long? pwMasterProductId, CogsDto defaults, List<CogsDto> details)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);

            if (pwMasterVariantId.HasValue)
            {
                using (var transaction = _connectionWrapper.StartTransactionForScope())
                {
                    var context = MasterVariantUpdateContext.Make(pwMasterVariantId, defaults, details);
                    var dateBlockContexts =
                        CogsDateBlockContext.Make(
                            defaults, details, PwShop.CurrencyId, null, pwMasterVariantId);

                    UpdateCogsForMasterVariant(context);
                    UpdateGoodsOnHandForMasterVariant(dateBlockContexts);                    
                    UpdateOrderLinesAndReportEntries(dateBlockContexts);

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

                    var dateBlockContexts = 
                        CogsDateBlockContext.Make(
                            defaults, details, PwShop.CurrencyId, pwMasterProductId, null);

                    UpdateGoodsOnHandForMasterProduct(dateBlockContexts);
                    UpdateOrderLinesAndReportEntries(dateBlockContexts);

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

        public void UpdateGoodsOnHandForMasterVariant(IList<CogsDateBlockContext> dateBlockContexts)
        {
            var repository = _multitenantFactory.MakeCogsEntryRepository(this.PwShop);
            repository.DeleteCogsCalcByMasterVariant(dateBlockContexts.First());
            foreach (var context in dateBlockContexts)
            {
                repository.InsertCogsCalcByMasterVariant(context);
            }
        }

        public void UpdateGoodsOnHandForMasterProduct(IList<CogsDateBlockContext> dateBlockContexts)
        {
            var repository = _multitenantFactory.MakeCogsEntryRepository(this.PwShop);
            repository.DeleteCogsCalcByMasterProduct(dateBlockContexts.First());
            foreach (var context in dateBlockContexts)
            {
                repository.InsertCogsCalcByMasterProduct(context);
            }
        }


        // Report Entries for Non-PickList
        public void UpdateOrderLinesAndReportEntries(IList<CogsDateBlockContext> dateBlockContexts)
        {
            var cogsDownstreamRepository = _multitenantFactory.MakeCogsDownstreamRepository(PwShop);

            foreach (var dateBlockContext in dateBlockContexts)
            {
                cogsDownstreamRepository.UpdateOrderLines(dateBlockContext);
            }

            cogsDownstreamRepository.RefreshReportEntryData();
        }


        
        // Updates both the CoGS and the Downstream
        public void UpdateCogsForPickList(long pickListId, CogsDto cogs)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDownstreamRepository(PwShop);

            using (var trans = _connectionWrapper.StartTransactionForScope())
            {
                // Update Pick List Default Cogs
                cogsEntryRepository.UpdatePickListDefaultCogs(pickListId, cogs);

                // Update the Order Lines for the Pick List
                var context = CogsDateBlockContext.Make(cogs, this.PwShop.CurrencyId, pickListId);

                cogsUpdateRepository.UpdateOrderLinesPickList(context);

                // Update Goods on Hand
                cogsEntryRepository.DeleteCogsCalcByPickList(context);
                cogsEntryRepository.InsertCogsCalcByPickList(context);

                // Update the Report Entries
                cogsUpdateRepository.RefreshReportEntryData();

                trans.Commit();
            }
        }


        // In-memory computation for the Order Refresh Step
        public decimal AssignUnitCogsToLineItem(
                IList<CogsDateBlockContext> contexts, ShopifyOrderLineItem lineItem)
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

