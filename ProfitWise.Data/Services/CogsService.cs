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
        private readonly MultitenantFactory _multitenantFactory;
        private readonly CurrencyService _currencyService;
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly IPushLogger _logger;

        public PwShop PwShop { get; set; }

        public CogsService(
                MultitenantFactory multitenantFactory, 
                CurrencyService currencyService, 
                ConnectionWrapper connectionWrapper, 
                IPushLogger logger)
        {
            _multitenantFactory = multitenantFactory;
            _currencyService = currencyService;
            _connectionWrapper = connectionWrapper;
            _logger = logger;
        }

        
       // Top-level orchestration functions
        public void SaveCogsForMasterVariant(long pwMasterVariantId, CogsDto defaults, List<CogsDto> details)
        {
            using (var transaction = _connectionWrapper.InitiateTransaction())
            {
                var context = MasterVariantUpdateContext.Make(pwMasterVariantId, defaults, details);
                var dateBlockContexts =
                    CogsDateBlockContext.Make(
                        defaults, details, PwShop.CurrencyId, pwMasterVariantId: pwMasterVariantId);

                SaveCogsDataEntryForMasterVariant(context);
                UpdateGoodsOnHandForMasterVariant(dateBlockContexts);                    
                UpdateOrderLinesCogs(dateBlockContexts);

                var downStreamRepository = _multitenantFactory.MakeCogsDownstreamRepository(PwShop);
                downStreamRepository.UpdateReportEntryLedger(
                        new EntryRefreshContext { PwMasterVariantId = pwMasterVariantId });

                transaction.Commit();
            }
        }
        
        public void SaveCogsForMasterProduct(
                long pwMasterProductId, CogsDto defaults, List<CogsDto> details)
        {
            using (var transaction = _connectionWrapper.InitiateTransaction())
            {
                var repository = _multitenantFactory.MakeVariantRepository(PwShop);
                var masterVariantIds = repository.RetrieveMasterVariantIdsForMasterProduct(pwMasterProductId);

                foreach (var masterVariantId in masterVariantIds)
                {
                    var context = MasterVariantUpdateContext.Make(masterVariantId, defaults, details);
                    SaveCogsDataEntryForMasterVariant(context);
                }

                var dateBlockContexts =
                    CogsDateBlockContext.Make(
                        defaults, details, PwShop.CurrencyId, pwMasterProductId: pwMasterProductId);

                UpdateGoodsOnHandForMasterProduct(dateBlockContexts);
                UpdateOrderLinesCogs(dateBlockContexts);

                var downStreamRepository = _multitenantFactory.MakeCogsDownstreamRepository(PwShop);
                downStreamRepository.UpdateReportEntryLedger(
                        new EntryRefreshContext { PwMasterProductId = pwMasterProductId});

                transaction.Commit();
            }
        }

        // Data Entry storing functionality
        private void SaveCogsDataEntryForMasterVariant(MasterVariantUpdateContext context)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);

            // Save the Master Variant CoGS Defaults
            cogsEntryRepository.UpdateMasterVariantDefaultCogs(
                    context.PwMasterVariantId, context.Defaults, context.HasDetails);

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


        // Goods on Hand Date Range-cached data
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
        public void UpdateOrderLinesCogs(IList<CogsDateBlockContext> dateBlockContexts)
        {
            var cogsDownstreamRepository = _multitenantFactory.MakeCogsDownstreamRepository(PwShop);

            foreach (var dateBlockContext in dateBlockContexts)
            {
               cogsDownstreamRepository.UpdateOrderLines(dateBlockContext);
            }
        }
        
        
        // Updates both the CoGS and the Downstream
        public void SaveCogsForPickList(long pickListId, CogsDto defaults, List<CogsDto> details)
        {
            var cogsEntryRepository = _multitenantFactory.MakeCogsEntryRepository(PwShop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDownstreamRepository(PwShop);

            using (var trans = _connectionWrapper.InitiateTransaction())
            {
                // First clean out the old
                cogsEntryRepository.DeleteCogsDetailByPickList(pickListId);

                // Update Pick List Default Cogs
                var cogsDetail = details != null && details.Any();
                cogsEntryRepository
                    .UpdatePickListDefaultCogs(pickListId, defaults.ToPwCogsDetail(null), cogsDetail);

                foreach (var detail in details)
                {
                    cogsEntryRepository.InsertCogsDetailByPickList(pickListId, detail.ToPwCogsDetail(null));
                }

                // Update the Order Lines for the Pick List
                var contexts = CogsDateBlockContext.Make(defaults, details, this.PwShop.CurrencyId, pwPickListId: pickListId);
                foreach (var context in contexts)
                {
                    cogsUpdateRepository.UpdateOrderLinesPickList(context);
                }

                // Update Goods on Hand
                cogsEntryRepository.DeleteCogsCalcByPickList(pickListId);
                foreach (var context in contexts)
                {
                    cogsEntryRepository.InsertCogsCalcByPickList(context);
                }

                // Update the Report Entries
                cogsUpdateRepository.UpdateReportEntryLedger(
                        new EntryRefreshContext { PwPickListId = pickListId });

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
                    $"Unable to locate OrderLineCogsContext for Order Line: {lineItem.ShopifyOrderLineId} /" +
                    $"Order Date: {lineItem.OrderDate}");
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
                return lineItem.UnitPrice * context.CogsPercentOfUnitPrice / 100.0m;
            }
        }


        public void RecomputeCogsFullDatalog()
        {
            var service = _multitenantFactory.MakeCatalogRetrievalService(this.PwShop);
            var fullcatalog = service.RetrieveFullCatalog();
            var downstreamRepository = _multitenantFactory.MakeCogsDownstreamRepository(this.PwShop);
            
            foreach (var masterVariant in fullcatalog.SelectMany(x => x.MasterVariants))
            {
                using (var transaction = _connectionWrapper.InitiateTransaction())
                {
                    CogsDto defaults = new CogsDto()
                    {
                        CogsAmount = masterVariant.CogsAmount,
                        CogsCurrencyId = masterVariant.CogsCurrencyId,
                        CogsMarginPercent = masterVariant.CogsMarginPercent,
                        CogsTypeId = masterVariant.CogsTypeId
                    };

                    var dateBlockContexts =
                        CogsDateBlockContext.Make(
                            defaults, null, PwShop.CurrencyId, null, masterVariant.PwMasterVariantId);

                    // Refresh CoGS for Master Variants
                    UpdateGoodsOnHandForMasterVariant(dateBlockContexts);

                    // Refresh Order Line CoGS
                    UpdateOrderLinesCogs(dateBlockContexts);

                    // Refresh the Ledger
                    var refreshContext = new EntryRefreshContext();
                    refreshContext.PwMasterVariantId = masterVariant.PwMasterVariantId;
                    downstreamRepository.DeleteEntryLedger(refreshContext);
                    downstreamRepository.RefreshEntryLedger(refreshContext);

                    transaction.Commit();

                    _logger.Info($"Completed Master Variant {masterVariant.PwMasterVariantId}");
                }
            }
        }
    }
}


