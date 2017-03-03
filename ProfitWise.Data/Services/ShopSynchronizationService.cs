using System.Configuration;
using System.Data;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories.System;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
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

        public ShopSynchronizationService(
                    CurrencyService currencyService, 
                    ShopRepository pwShopRepository, 
                    MultitenantFactory factory,
                    IPushLogger logger,
                    ConnectionWrapper connectionWrapper)
        {
            _currencyService = currencyService;
            _pwShopRepository = pwShopRepository;
            _factory = factory;
            _logger = logger;
            _connectionWrapper = connectionWrapper;
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

        public PwShop UpsertShop(string shopOwnerUserId, Shop shop)
        {
            var pwShop = _pwShopRepository.RetrieveByUserId(shopOwnerUserId);
            if (pwShop == null)
            {
                var shopId = CreateShop(shopOwnerUserId, shop);
                return _pwShopRepository.RetrieveByShopId(shopId);
            }
            else
            {
                UpdateShop(shopOwnerUserId, shop.Currency, shop.TimeZone);
                return pwShop;;
            }
        }

        public int CreateShop(string shopOwnerUserId, Shop shop)
        {
            var orderStartOffsetMonths =
                    ConfigurationManager.AppSettings.GetAndTryParseAsInt("InitialOrderStartDateOffsetMonths", 3);

            var currencyId = _currencyService.AbbreviationToCurrencyId(shop.Currency);
            var newShop = PwShop.Make(
                shopOwnerUserId, shop.Id, currencyId, shop.TimeZone, shop.Domain, orderStartOffsetMonths);

            newShop.PwShopId = _pwShopRepository.Insert(newShop);

            _logger.Info($"Created new Shop - UserId: {newShop.ShopOwnerUserId}, " +
                         $"CurrencyId: {newShop.CurrencyId}, " +
                         $"ShopifyShopId: {newShop.ShopifyShopId}, " +
                         $"StartingDateForOrders: {newShop.StartingDateForOrders}");

            // Ensure the proper Batch State exists for Shop
            var profitWiseBatchStateRepository = _factory.MakeBatchStateRepository(newShop);
            var state = new PwBatchState
            {
                PwShopId = newShop.PwShopId,
            };
            profitWiseBatchStateRepository.Insert(state);
            _logger.Info($"Created Batch State for Shop - UserId: {newShop.ShopOwnerUserId}");

            return newShop.PwShopId;
        }

        public void UpdateShop(string userId, string currencySymbol, string timezone)
        {
            var pwShop = _pwShopRepository.RetrieveByUserId(userId);
            var currencyId = _currencyService.AbbreviationToCurrencyId(currencySymbol);

            pwShop.CurrencyId = currencyId;
            pwShop.TimeZone = timezone;
            _pwShopRepository.Update(pwShop);
            _pwShopRepository.UpdateIsAccessTokenValid(pwShop.PwShopId, true);

            _logger.Debug($"Updated Shop - UserId: {pwShop.ShopOwnerUserId}, " +
                            $"CurrencyId: {pwShop.CurrencyId}, " +
                            $"TimeZone: {pwShop.TimeZone} - and set IsAccessTokenValid = true");
        }
    }
}
