using System;
using System.Configuration;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Helpers;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;


namespace ProfitWise.Data.ProcessSteps
{
    public class ShopRefreshService
    {
        private readonly BatchLogger _pushLogger;
        private readonly ShopRepository _shopDataRepository;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly ShopOrchestrationService _shopOrchestrationService;

        public ShopRefreshService(
                    BatchLogger pushLogger, 
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

        public bool Execute(ShopifyCredentials credentials)
        {            
            _pushLogger.Info($"Shop Refresh Service for Shop: {credentials.ShopDomain}, UserId: {credentials.ShopOwnerUserId}");
            var shop = _shopDataRepository.RetrieveByUserId(credentials.ShopOwnerUserId);
            
            // Routing check on existing Shop status flags
            if (shop.IsProfitWiseInstalled == false)
            {
                if (shop.UninstallDateTime.HasValue &&
                    shop.UninstallDateTime.Value.AddHours(UninstallationFinalizeHours) < DateTime.Now)
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
            if (shop.IsAccessTokenValid == false)
            {
                _pushLogger.Warn($"Shop {shop.PwShopId} has an invalid Access Token - skipping Refresh");
                return false;
            }
            if (shop.IsBillingValid == false)
            {
                _pushLogger.Warn($"Shop {shop.PwShopId} has an invalid Billing - skipping Refresh");
                return false;
            }

            // Invoke Shopify API to get the latest Billing Status
            if (!_shopOrchestrationService.SyncAndValidateBilling(shop))
            {
                _pushLogger.Warn($"Shop {shop.PwShopId} has Billing become invalid - skipping Refresh");
                return false;
            }
            
            // Update Shop with the latest from Shopify API
            var shopApiRepository = _apiRepositoryFactory.MakeShopApiRepository(credentials);
            var shopFromShopify = shopApiRepository.Retrieve();
            _shopOrchestrationService.UpdateShop(credentials.ShopOwnerUserId, shopFromShopify.Currency, shopFromShopify.TimeZone);

            return true;
        }
    }
}
