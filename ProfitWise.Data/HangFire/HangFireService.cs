using System;
using System.Configuration;
using Hangfire;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Processes;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Data.HangFire
{
    public class HangFireService
    {
        private readonly IPushLogger _logger;
        private readonly PwShopRepository _shopRepository;
        private readonly MultitenantFactory _multitenantFactory;
        
        private readonly string 
            _shopRefreshInterval = ConfigurationManager.AppSettings
                    .GetAndTryParseAsString("ShopRefreshInterval", "0 * * * *");

        private readonly string 
            _exchangeRefreshInterval = ConfigurationManager.AppSettings
                .GetAndTryParseAsString("ExchangeRefreshInterval", "0 2 * * *");

        private readonly string
            _cleanupServiceInterval = ConfigurationManager.AppSettings
                .GetAndTryParseAsString("CleanupServiceInterval", "* * * * *");

        private readonly string
            _machineTimeZone = ConfigurationManager.AppSettings
                .GetAndTryParseAsString("Machine_TimeZone", "(GMT-06:00) Central Time (US &amp; Canada)");

        private TimeZoneInfo HangFireTimeZone =>
            //TimeZoneInfo.FindSystemTimeZoneById(_machineTimeZone) ??  // Doesn't work???
            TimeZoneInfo.Local;


        public HangFireService(
                IPushLogger logger, 
                PwShopRepository shopRepository, 
                MultitenantFactory multitenantFactory)
        {
            _logger = logger;
            _shopRepository = shopRepository;
            _multitenantFactory = multitenantFactory;
        }
        

        public string TriggerInitialShopRefresh(string userId)
        {
            _logger.Info($"Scheduling Initial Shop Refresh for User {userId}");
            var jobId =  BackgroundJob.Enqueue<ShopRefreshProcess>(x => x.InitialShopRefresh(userId));
            
            var shop = _shopRepository.RetrieveByUserId(userId);
            var batchRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            batchRepository.UpdateInitialRefreshJobId(jobId);
            return jobId;
        }

        public string ScheduleRoutineShopRefresh(string userId)
        {
            var jobId = $"ShopRefreshProcess:{userId}"; 
            _logger.Info($"Scheduling Routine Shop Refresh for User {userId} / Job Id {jobId}");

            RecurringJob
                .AddOrUpdate<ShopRefreshProcess>(
                    jobId, x => x.RoutineShopRefresh(userId), _shopRefreshInterval, HangFireTimeZone);

            var shop = _shopRepository.RetrieveByUserId(userId);
            var batchRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            batchRepository.UpdateRoutineRefreshJobId(jobId);
            return jobId;
        }

        public void ScheduleExchangeRateRefresh()
        {
            var jobId = "ExchangeRateRefresh";
            _logger.Info($"Scheduling ExchangeRateRefreshProcess");

            RecurringJob.AddOrUpdate<ExchangeRateRefreshProcess>(
                    jobId, x => x.Execute(), _exchangeRefreshInterval, HangFireTimeZone);
        }

        public void ScheduleSystemCleanupProcess()
        {
            var jobId = "SystemCleanupProcess";
            _logger.Info($"Scheduling SystemCleanupProcess");

            RecurringJob.AddOrUpdate<SystemCleanupProcess>(
                    jobId, x => x.Execute(), _cleanupServiceInterval, HangFireTimeZone);
        }

        public void KillRecurringJob(string jobId)
        {
            RecurringJob.RemoveIfExists(jobId);
        }

        public void KillBackgroundJob(string jobId)
        {
            BackgroundJob.Delete(jobId);
        }
    }
}
