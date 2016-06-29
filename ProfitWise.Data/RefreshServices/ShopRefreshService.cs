﻿using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Utilities.General;

namespace ProfitWise.Data.RefreshServices
{
    public class ShopRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly ShopRepository _shopRepository;

        public ShopRefreshService(IPushLogger pushLogger, ShopRepository shopRepository)
        {
            _pushLogger = pushLogger;
            _shopRepository = shopRepository;
        }

        public int Execute(string userId)
        {
            _pushLogger.Info($"{this.ClassAndMethodName()} - for UserId : {userId}");

            var shop = _shopRepository.RetrieveByUserId(userId);

            if (shop == null)
            {
                return _shopRepository.Insert(new ShopifyShop {UserId = userId});
            }
            else
            {
                return shop.ShopId;
            }
        }
    }
}