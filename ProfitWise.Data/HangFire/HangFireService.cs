using System;
using System.Configuration;
using Hangfire;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Processes;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Repositories.System;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.HttpClient;

namespace ProfitWise.Data.HangFire
{
    public class HangFireService
    {
        private readonly IPushLogger _logger;
        private readonly ShopRepository _shopRepository;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly IShopifyCredentialService _shopifyCredentialService;

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

        private TimeZoneInfo HangFireTimeZone => TimeZoneInfo.Local;


        public HangFireService(
                IShopifyCredentialService shopifyCredentialService, 
                IPushLogger logger, 
                ShopRepository shopRepository, 
                MultitenantFactory multitenantFactory)
        {
            _shopifyCredentialService = shopifyCredentialService;
            _logger = logger;
            _shopRepository = shopRepository;
            _multitenantFactory = multitenantFactory;
        }
        

        public string AddOrUpdateInitialShopRefresh(string userId)
        {
            var shop = _shopRepository.RetrieveByUserId(userId);
            var batchRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var batch = batchRepository.Retrieve();

            if (batch.RoutineRefreshJobId != null)
            {
                _logger.Info($"Aborting addition of Initial Shop Refresh; " + 
                        $"Routine Job Refresh {batch.RoutineRefreshJobId} exists already");
                return batch.RoutineRefreshJobId;
            }

            _logger.Info($"Scheduling Initial Shop Refresh for Shop: UserId: {userId}");
            var jobId =  BackgroundJob.Enqueue<ShopRefreshProcess>(x => x.InitialShopRefresh(userId));            
            batchRepository.UpdateInitialRefreshJobId(jobId);

            return jobId;
        }

        public string AddOrUpdateRoutineShopRefresh(string userId)
        {
            var shopifyFromClaims = _shopifyCredentialService.Retrieve(userId);
            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(
                    $"ShopifyCredentialService unable to Retrieve for Shop: {shopifyFromClaims.ShopDomain}, UserId: {userId} - {shopifyFromClaims.Message}");
            }

            var shopifyClientCredentials = new ShopifyCredentials()
            {
                ShopOwnerUserId = shopifyFromClaims.ShopOwnerUserId,
                ShopDomain = shopifyFromClaims.ShopDomain,
                AccessToken = shopifyFromClaims.AccessToken,
            };

            var jobId = $"ShopRefreshProcess:{userId}"; 
            _logger.Info($"Scheduling Routine Shop Refresh for Shop: {shopifyClientCredentials.ShopDomain}, UserId: {userId}, Job Id: {jobId}");

            RecurringJob
                .AddOrUpdate<ShopRefreshProcess>(
                    jobId, x => x.RoutineShopRefresh(userId), _shopRefreshInterval, HangFireTimeZone);

            using (var transaction = _shopRepository.InitiateTransaction())
            {
                var shop = _shopRepository.RetrieveByUserId(userId);
                var batchRepository = _multitenantFactory.MakeBatchStateRepository(shop);
                batchRepository.UpdateRoutineRefreshJobId(jobId);
                transaction.Commit();
            }
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

        public void KillRoutineRefresh(string userId)
        {
            var shop = _shopRepository.RetrieveByUserId(userId);
            var batchRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var batch = batchRepository.Retrieve();

            if (batch.RoutineRefreshJobId != null)
            {
                RecurringJob.RemoveIfExists(batch.RoutineRefreshJobId);
                batchRepository.UpdateRoutineRefreshJobId(null);
            }
        }

        public void KillInitialRefresh(string userId)
        {
            var shop = _shopRepository.RetrieveByUserId(userId);
            var batchRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var batch = batchRepository.Retrieve();
            
            if (batch.InitialRefreshJobId != null)
            {
                BackgroundJob.Delete(batch.InitialRefreshJobId);
                batchRepository.UpdateInitialRefreshJobId(null);
            }
        }
    }
}
