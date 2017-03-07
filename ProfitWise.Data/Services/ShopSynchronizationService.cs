using System.Configuration;
using System.Data;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories.System;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace ProfitWise.Data.Services
{
    public class ShopSynchronizationService
    {
        private readonly CurrencyService _currencyService;
        private readonly ShopRepository _pwShopRepository;
        private readonly MultitenantFactory _factory;
        private readonly IPushLogger _logger;
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly ApiRepositoryFactory _apifactory;


        private readonly 
            int _orderStartOffsetMonths = ConfigurationManager
                .AppSettings.GetAndTryParseAsInt("InitialOrderStartDateOffsetMonths", 3);

        public ShopSynchronizationService(
                    CurrencyService currencyService, 
                    ShopRepository pwShopRepository, 
                    MultitenantFactory factory,
                    IPushLogger logger,
                    ConnectionWrapper connectionWrapper, 
                    ApiRepositoryFactory apifactory)
        {
            _currencyService = currencyService;
            _pwShopRepository = pwShopRepository;
            _factory = factory;
            _logger = logger;
            _connectionWrapper = connectionWrapper;
            _apifactory = apifactory;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }
        
        public bool ExistsButDisabled(string shopOwnerUserId)
        {
            var pwShop = _pwShopRepository.RetrieveByUserId(shopOwnerUserId);
            return (pwShop != null && pwShop.IsShopEnabled == false);
        }
        
        public PwShop CreateShop(string shopOwnerUserId, Shop shop, ShopifyCredentials credentials)
        {
            // Create the Shop record in SQL
            var currencyId = _currencyService.AbbreviationToCurrencyId(shop.Currency);
            var newShop = PwShop.Make(
                shopOwnerUserId, shop.Id, currencyId, shop.TimeZone, shop.Domain, _orderStartOffsetMonths);
            newShop.PwShopId = _pwShopRepository.Insert(newShop);
            _logger.Info($"Created new Shop - UserId: {newShop.ShopOwnerUserId}");

            // Create the Batch State for Shop
            var profitWiseBatchStateRepository = _factory.MakeBatchStateRepository(newShop);
            var state = new PwBatchState
            {
                PwShopId = newShop.PwShopId,
            };
            profitWiseBatchStateRepository.Insert(state);
            _logger.Info($"Created Batch State for Shop - UserId: {newShop.ShopOwnerUserId}");

            // Create the Webhook via Shopify API
            var apiRepository = _apifactory.MakeWebhookApiRepository(credentials);            
            var request = Webhook.MakeUninstallHookRequest();
            var webhook = apiRepository.Subscribe(request);

            // Store the Webhook Id 
            _pwShopRepository.UpdateShopifyUninstallId(shopOwnerUserId, webhook.Id);
            return newShop;
        }
    

        public void UpdateShop(string userId, string currencySymbol, string timezone)
        {
            var pwShop = _pwShopRepository.RetrieveByUserId(userId);
            var currencyId = _currencyService.AbbreviationToCurrencyId(currencySymbol);

            pwShop.CurrencyId = currencyId;
            pwShop.TimeZone = timezone;
            _pwShopRepository.Update(pwShop);
            _pwShopRepository.UpdateIsAccessTokenValid(pwShop.PwShopId, true);

            _logger.Debug($"Updated Shop - UserId: {pwShop.ShopOwnerUserId}");
        }
    }
}
