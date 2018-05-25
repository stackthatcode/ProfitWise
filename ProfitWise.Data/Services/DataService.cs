﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.GoodsOnHand;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Services
{
    // Intended for enabiling the asynchronous scheduling of data exports
    [Intercept(typeof(ShopRequired))]
    public class DataService
    {
        private readonly MultitenantFactory _factory;
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly TimeZoneTranslator _timeZoneTranslator;

        public PwShop PwShop { get; set; }

        public DataService(
                MultitenantFactory factory, 
                ConnectionWrapper connectionWrapper, 
                TimeZoneTranslator timeZoneTranslator)
        {
            _factory = factory;
            _connectionWrapper = connectionWrapper;
            _timeZoneTranslator = timeZoneTranslator;
        }

        public List<GroupedTotal> ProfitabilityDetails(
                long reportId, ReportGrouping grouping, Model.Profit.ColumnOrdering ordering, 
                int pageNumber, int pageSize)
        {
            var repository = _factory.MakeReportRepository(PwShop);
            var queryRepository = _factory.MakeProfitRepository(PwShop);
            var report = repository.RetrieveReport(reportId);

            queryRepository.PopulateQueryStub(reportId);
            var queryContext = new TotalQueryContext(PwShop)
            {
                PwReportId = reportId,
                StartDate = report.StartDate,
                EndDate = report.EndDate,
                Grouping = grouping,
                Ordering = ordering,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };

            var totals = queryRepository.RetrieveTotalsByContext(queryContext);
                        
            var allProfit = totals.Sum(x => x.TotalProfit);

            // Compute Profit % for each line item
            foreach (var groupTotal in totals)
            {
                groupTotal.ProfitPercentage =
                    allProfit == 0 ? 0m : (groupTotal.TotalProfit / allProfit) * 100m;
            }
              
            return totals;
        }


        // Overwhelmingly similar to ProfitabilityDetails, although with the addition
        // several key calculations and descriptive fields.
        public List<ExportDetailRow> ProfitabilityDetailAllFields(long reportId)
        {
            var reportRepository = _factory.MakeReportRepository(PwShop);
            var queryRepository = _factory.MakeProfitRepository(PwShop);
            queryRepository.PopulateQueryStub(reportId);

            var report = reportRepository.RetrieveReport(reportId);
            var queryContext = new TotalQueryContext(PwShop)
            {
                PwReportId = reportId,
                StartDate = report.StartDate,
                EndDate = report.EndDate,
            };

            var totals = queryRepository.RetreiveTotalsForExportDetail(queryContext);            
            var allProfit = totals.Sum(x => x.TotalProfit);
            totals.ForEach(x => x.ProfitPercentage = allProfit == 0 
                                    ? 0m : Math.Round((x.TotalProfit / allProfit) * 100m, 4));

            return totals;
        }

        public List<Details> GoodsOnHandDetails(
                long reportId, Model.GoodsOnHand.ColumnOrdering ordering, int pageNumber, int pageSize,
                ReportGrouping? grouping = null, string productType = null, string vendor = null, 
                long? pwProductId = null)
        {
            var repository = _factory.MakeGoodsOnHandRepository(PwShop);
            var details = repository.RetrieveDetails(
                reportId, grouping.Value, ordering, pageNumber, pageSize,
                productType, vendor, pwProductId);

            return details;
        }
    }
}

