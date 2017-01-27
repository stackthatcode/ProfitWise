using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Model;

namespace ProfitWise.Data.Services
{
    public class ShopSynchronizationService
    {
        private readonly CurrencyService _currencyService;
        private readonly PwShopRepository _pwShopRepository;
        private readonly IPushLogger _logger;

        public ShopSynchronizationService(
                    CurrencyService currencyService, 
                    PwShopRepository pwShopRepository, 
                    IPushLogger logger)
        {
            _currencyService = currencyService;
            _pwShopRepository = pwShopRepository;
            _logger = logger;
        }

        // ShopOwnerUserId is an ASP.NET User Id
        public bool IsShopExistingButDisabled(string shopOwnerUserId)
        {
            var pwShop = _pwShopRepository.RetrieveByUserId(shopOwnerUserId);
            return (pwShop != null && pwShop.IsShopEnabled == false);
        }

        // Either creates a new Shop record, or updates existing with Currency and TimeZone
        // NOTE: ShopOwnerUserId is an ASP.NET User Id
        public void RefreshShop(string shopOwnerUserId, Shop shop)
        {
            var currencyId = _currencyService.AbbreviationToCurrencyId(shop.Currency);
            var pwShop = _pwShopRepository.RetrieveByUserId(shopOwnerUserId);

            if (pwShop == null)
            {
                var newShop = PwShop.Make(shopOwnerUserId, shop.Id, currencyId, shop.TimeZone);
                newShop.PwShopId = _pwShopRepository.Insert(newShop);

                _logger.Info($"Created new Shop - UserId: {newShop.ShopOwnerUserId}, " +
                             $"CurrencyId: {newShop.CurrencyId}, " +
                             $"ShopifyShopId: {newShop.ShopifyShopId}, " +
                             $"StartingDateForOrders: {newShop.StartingDateForOrders}");
            }
            else
            {
                UpdateShopTimeZoneAndCurrency(pwShop, currencyId, shop.TimeZone);
            }
        }

        public void UpdateShopTimeZoneAndCurrency(PwShop pwShop, int currencyId, string timezone)
        {
            pwShop.CurrencyId = currencyId;
            pwShop.TimeZone = timezone;
            _pwShopRepository.Update(pwShop);
            _pwShopRepository.UpdateIsAccessTokenValid(pwShop.PwShopId, true);

            _logger.Info($"Updated Shop - UserId: {pwShop.ShopOwnerUserId}, " +
                        $"CurrencyId: {pwShop.CurrencyId}, " +
                        $"TimeZone: {pwShop.TimeZone} - and set IsAccessTokenValid = true");
        }
    }
}
