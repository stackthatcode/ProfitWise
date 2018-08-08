using System;
using System.Configuration;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;


namespace ProfitWise.Data.ProcessSteps
{
    public class ShopRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly ShopRepository _shopDataRepository;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly ShopOrchestrationService _shopOrchestrationService;

        public ShopRefreshService(
                    IPushLogger pushLogger, 
                    ShopRepository shopDataRepository,
                    ApiRepositoryFactory apiRepositoryFactory,
                    ShopOrchestrationService shopSynchronizationService)
        {
            _pushLogger = pushLogger;
            _shopDataRepository = shopDataRepository;
            _apiRepositoryFactory = apiRepositoryFactory;
            _shopOrchestrationService = shopSynchronizationService;
        }

        public readonly int UninstallationFinalizeHours =
            ConfigurationManager.AppSettings.GetAndTryParseAsInt("UninstallationFinalizeHours", 2);

        public readonly int MaximumShopifyApiHttp401Count =
            ConfigurationManager.AppSettings.GetAndTryParseAsInt("MaximumShopifyApiHttp401Count", 10);

        public bool Execute(ShopifyCredentials credentials)
        {            
            _pushLogger.Info(
                $"Shop Refresh Service for Shop: {credentials.ShopDomain}, " +
                $"UserId: {credentials.ShopOwnerUserId}");

            var shop = _shopDataRepository.RetrieveByUserId(credentials.ShopOwnerUserId);
            
            // Routing check on existing Shop status flags
            if (shop.IsProfitWiseInstalled == false)
            {
                if (shop.UninstallDateTime.HasValue &&
                    shop.UninstallDateTime.Value.AddHours(UninstallationFinalizeHours) < DateTime.UtcNow)
                {
                    _shopOrchestrationService.FinalizeUninstallation(shop.PwShopId);
                    _pushLogger.Info($"Finalizing Uninstallation process for {shop.PwShopId}");
                }
                else
                {
                    _pushLogger.Warn($"Shop {shop.PwShopId} is not installed - skipping Refresh");
                }
                return false;
            }

            if (shop.FailedAuthorizationCount > MaximumShopifyApiHttp401Count)
            {
                _pushLogger.Warn(
                    $"Shop {shop.PwShopId} has an invalid Access Token after {MaximumShopifyApiHttp401Count} failed attempts" +
                    $" - flagging, uninstalling and skipping Refresh");

                _shopDataRepository.UpdateIsAccessTokenValid(shop.PwShopId, false);
                _shopDataRepository.UpdateIsProfitWiseInstalled(shop.PwShopId, false, DateTime.UtcNow);
                return false;
            }

            // Invoke Shopify API to get the latest Billing Status
            if (!_shopOrchestrationService.SyncAndValidateBilling(shop))
            {
                _pushLogger.Warn(
                    $"Shop {shop.PwShopId} Billing was just invalidated - skipping Refresh");
                return false;
            }

            // Webhook installation
            _shopOrchestrationService.UpsertAllWebhooks(shop.ShopOwnerUserId, credentials);

            // Update Shop with the latest from Shopify API
            var shopApiRepository = _apiRepositoryFactory.MakeShopApiRepository(credentials);
            var shopFromShopify = shopApiRepository.Retrieve();

            _shopOrchestrationService.UpdateShopAndAccessTokenValid(
                credentials.ShopOwnerUserId, shopFromShopify.Currency, shopFromShopify.TimeZoneIana);

            return true;
        }
    }
}
