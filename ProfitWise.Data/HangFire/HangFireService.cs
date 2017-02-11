using System;
using System.Configuration;
using Hangfire;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Processes;
using ProfitWise.Data.Repositories;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.HttpClient;

namespace ProfitWise.Data.HangFire
{
    public class HangFireService
    {
        private readonly IPushLogger _logger;
        private readonly PwShopRepository _shopRepository;
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

        private TimeZoneInfo HangFireTimeZone =>
            //TimeZoneInfo.FindSystemTimeZoneById(_machineTimeZone) ??  // Doesn't work???
            TimeZoneInfo.Local;


        public HangFireService(
                IShopifyCredentialService shopifyCredentialService, 
                IPushLogger logger, 
                PwShopRepository shopRepository, 
                MultitenantFactory multitenantFactory)
        {
            _shopifyCredentialService = shopifyCredentialService;
            _logger = logger;
            _shopRepository = shopRepository;
            _multitenantFactory = multitenantFactory;
        }
        

        public string TriggerInitialShopRefresh(string userId)
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

            _logger.Info($"Scheduling Initial Shop Refresh for Shop: {shopifyClientCredentials.ShopDomain}, UserId: {userId}");
            var jobId =  BackgroundJob.Enqueue<ShopRefreshProcess>(x => x.InitialShopRefresh(userId));
            
            var shop = _shopRepository.RetrieveByUserId(userId);
            var batchRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            batchRepository.UpdateInitialRefreshJobId(jobId);
            return jobId;
        }

        public string ScheduleRoutineShopRefresh(string userId)
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
