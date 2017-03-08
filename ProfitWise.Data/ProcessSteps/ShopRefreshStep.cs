using System;
using System.Configuration;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Helpers;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;


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
            
            // Routine check on Shop Status
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
                _pushLogger.Warn($"Shop {shop.PwShopId} is has an invalid Access Token - skipping Refresh");
                return false;
            }

            // Update Shop with the latest from Shopify
            var shopApiRepository = _apiRepositoryFactory.MakeShopApiRepository(credentials);
            var shopFromShopify = shopApiRepository.Retrieve();
            _shopOrchestrationService.UpdateShop(credentials.ShopOwnerUserId, shopFromShopify.Currency, shopFromShopify.TimeZone);

            // Routine check on Billing Status            
            var charge = _shopOrchestrationService.SyncAndRetrieveCurrentCharge(credentials.ShopOwnerUserId);
            
            if (charge.LastStatus == ChargeStatus.Active)
            {
                _shopDataRepository.UpdateIsBillingValid(shop.PwShopId, true);
                return true; // All clear, nothing to do
            }

            if (charge.LastStatus == ChargeStatus.Accepted)
            {
                _shopOrchestrationService.ActivateCharge(
                        credentials.ShopOwnerUserId, charge.PwChargeId);
                return true;
            }

            // Charge Status is neither Active nor Accepted, thus set Billing Valid to false
            _shopDataRepository.UpdateIsBillingValid(shop.PwShopId, false);
            _pushLogger.Warn($"Shop {shop.PwShopId} Shopify Charge is {charge.LastStatus}");
            return false;
        }
    }
}
