using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories.System
{
    public class ShopRepository
    {
        private readonly ConnectionWrapper _connectionWrapper;        

        public ShopRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }


        public PwShop RetrieveByShopifyShopId(long shopifyShopId)
        {
            var query = @"SELECT * FROM vw_profitwiseshop WHERE ShopifyShopId = @shopifyShopId";
            return _connectionWrapper
                .Query<PwShop>(query, new { shopifyShopId })
                .FirstOrDefault();
        }
        

        public PwShop RetrieveByShopId(int pwShopId)
        {
            var query = @"SELECT * FROM vw_profitwiseshop WHERE PwShopId = @PwShopId";
            return _connectionWrapper
                .Query<PwShop>(query, new {@PwShopId = pwShopId })
                .FirstOrDefault();
        }

        public PwShop RetrieveByUserId(string shopOwnerUserId)
        {
            var query = @"SELECT * FROM vw_profitwiseshop WHERE ShopOwnerUserId = @ShopOwnerUserId";
            return _connectionWrapper
                .Query<PwShop>(query, new { ShopOwnerUserId = shopOwnerUserId })
                .FirstOrDefault();
        }

        public PwShop RetrieveShopByRecurringChargeId(string chargeId)
        {
            var query = @"SELECT * FROM vw_profitwiseshop WHERE PwShopId IN 
                        ( SELECT PwShopId FROM profitwiserecurringcharge 
                        WHERE ShopifyRecurringChargeId = @chargeId );";

            return _connectionWrapper.Query<PwShop>(query, new { chargeId }).FirstOrDefault();
        }

        
        public List<PwShop> RetrieveAllActiveShops()
        {
            var query =
                @"SELECT * FROM vw_profitwiseshop 
                WHERE IsAccessTokenValid = 1 
                AND IsProfitWiseInstalled = 1 
                AND LastBillingStatus = 3 
                ORDER BY PwShopId;";
            
            return _connectionWrapper
                .Query<PwShop>(query)
                .ToList();
        }

        public int Insert(PwShop shop)
        {
            var query =
                @"INSERT INTO profitwiseshop (
                    ShopOwnerUserId, ShopifyShopId, Domain, CurrencyId, TimeZone, 
                    IsAccessTokenValid, IsProfitWiseInstalled, IsDataLoaded, IsBillingValid,
                    StartingDateForOrders, UseDefaultMargin, DefaultMargin, ProfitRealization, 
                    DateRangeDefault, TempFreeTrialOverride, ShopifyUninstallId, MinIsNonZeroValue
                ) VALUES (
                    @ShopOwnerUserId, @ShopifyShopId, @Domain, @CurrencyId, @TimeZone,
                    @IsAccessTokenValid, @IsProfitWiseInstalled, @IsDataLoaded, @IsBillingValid,
                    @StartingDateForOrders, @UseDefaultMargin,  @DefaultMargin, @ProfitRealization, 
                    @DateRangeDefault, @TempFreeTrialOverride, @ShopifyUninstallId, @MinIsNonZeroValue );
                SELECT SCOPE_IDENTITY();";
            return _connectionWrapper
                .Query<int>(query, shop)
                .First();
        }

        public void Update(PwShop shop)
        {
            var query = @"UPDATE profitwiseshop 
                        SET CurrencyId = @CurrencyId, TimeZone = @TimeZone
                        WHERE PwShopId = @PwShopId";
            _connectionWrapper.Execute(query, shop);
        }

        public void IncrementFailedAuthorizationCount(int pwShopId)
        {
            var query = @"UPDATE profitwiseshop 
                        SET FailedAuthorizationCount = FailedAuthorizationCount + 1
                        WHERE PwShopId = @pwShopId";
            _connectionWrapper.Execute(query, new { pwShopId });
        }

        public void UpdateIsAccessTokenValid(int pwShopId, bool isAccessTokenValid)
        {
            var query = isAccessTokenValid
                ? @"UPDATE profitwiseshop SET IsAccessTokenValid = 1, FailedAuthorizationCount = 0 WHERE PwShopId = @pwShopId"
                : @"UPDATE profitwiseshop SET IsAccessTokenValid = 0 WHERE PwShopId = @pwShopId";
            
            _connectionWrapper.Execute(query, new { pwShopId });
        }

        public void UpdateIsProfitWiseInstalled(
                int pwShopId, bool isProfitWiseInstalled, DateTime? uninstallDateTime)
        {
            var query = @"UPDATE profitwiseshop 
                        SET IsProfitWiseInstalled = @isProfitWiseInstalled,
                            UninstallDateTime = @uninstallDateTime
                        WHERE PwShopId = @pwShopId";

            _connectionWrapper.Execute(query, new { pwShopId, isProfitWiseInstalled, uninstallDateTime });
        }

        public void UpdateIsDataLoaded(int pwShopId, bool isDataLoaded)
        {
            var query = @"UPDATE profitwiseshop 
                        SET IsDataLoaded = @isDataLoaded
                        WHERE PwShopId = @pwShopId";
            _connectionWrapper.Execute(query, new { pwShopId, isDataLoaded });
        }
        
        public void UpdateTempFreeTrialOverride(int pwShopId, int? days)
        {
            var query = @"UPDATE profitwiseshop 
                        SET [TempFreeTrialOverride] = @days
                        WHERE PwShopId = @pwShopId";
            _connectionWrapper.Execute(query, new { pwShopId, days });
        }

        public void UpdateShopifyUninstallId(string shopOwnerUserId, long? uninstallWebHookId)
        {
            var query = @"UPDATE profitwiseshop 
                        SET [ShopifyUninstallId] = @uninstallWebHookId
                        WHERE ShopOwnerUserId = @shopOwnerUserId";
            _connectionWrapper.Execute(query, new { shopOwnerUserId, uninstallWebHookId });
        }

        public void UpdateStartingDateForOrders(int pwShopId, DateTime startingDateForOrders)
        {
            var query = @"UPDATE profitwiseshop 
                        SET StartingDateForOrders = @startingDateForOrders
                        WHERE PwShopId = @pwShopId";
            _connectionWrapper.Execute(query, new { pwShopId, startingDateForOrders });
        }

        public void UpdateDefaultMargin(int pwShopId, bool useDefaultMargin, decimal defaultMargin)
        {
            var query = @"UPDATE profitwiseshop 
                        SET UseDefaultMargin = @useDefaultMargin, DefaultMargin = @defaultMargin  
                        WHERE PwShopId = @pwShopId";
            _connectionWrapper.Execute(query, new { pwShopId, useDefaultMargin, defaultMargin });
        }

        public void UpdateProfitRealization(int pwShopId, int profitRealization)
        {
            var query = @"UPDATE profitwiseshop 
                        SET ProfitRealization = @profitRealization
                        WHERE PwShopId = @pwShopId";
            _connectionWrapper.Execute(query, new { pwShopId, profitRealization });
        }

        public void UpdateDateRangeDefault(int pwShopId, int dateRangeDefault)
        {
            var query = @"UPDATE profitwiseshop 
                        SET DateRangeDefault = @dateRangeDefault
                        WHERE PwShopId = @pwShopId";
            _connectionWrapper.Execute(query, new { pwShopId, dateRangeDefault });
        }

        public void UpdateMinIsNonZeroValue(int pwShopId, int minIsNonZeroValue)
        {
            var query = @"UPDATE profitwiseshop 
                        SET MinIsNonZeroValue = @minIsNonZeroValue
                        WHERE PwShopId = @pwShopId";
            _connectionWrapper.Execute(query, new { pwShopId, minIsNonZeroValue });
        }


        public PwTourState RetreiveTourState(int pwShopId)
        {
            var query = @"SELECT * FROM tour(@pwShopId);";
            return _connectionWrapper
                    .Query<PwTourState>(query, new { pwShopId })
                    .FirstOrDefault();
        }

        public void InsertTour(int pwShopId)
        {
            var query = @"INSERT INTO tour(@PwShopId) VALUES (
                            @PwShopId,   1, 1, 1,   1, 1, 1,   1, 1, 1 )";
            _connectionWrapper.Execute(query, new { pwShopId });
        }

    }
}
