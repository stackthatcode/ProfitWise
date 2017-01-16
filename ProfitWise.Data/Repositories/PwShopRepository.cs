using System;
using System.Linq;
using MySql.Data.MySqlClient;
using Dapper;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories
{
    public class PwShopRepository
    {
        private readonly MySqlConnection _connection;

        public PwShopRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public PwShop RetrieveByShopId(int pwShopId)
        {
            var query = @"SELECT * FROM profitwiseshop WHERE PwShopId = @PwShopId";
            return _connection
                .Query<PwShop>(query, new {@PwShopId = pwShopId })
                .FirstOrDefault();
        }

        public PwShop RetrieveByUserId(string shopOwnerUserId)
        {
            var query = @"SELECT * FROM profitwiseshop WHERE ShopOwnerUserId = @ShopOwnerUserId";
            return _connection
                .Query<PwShop>(query, new { ShopOwnerUserId = shopOwnerUserId })
                .FirstOrDefault();
        }

        public int Insert(PwShop shop)
        {
            var query =
                @"INSERT INTO 
                    profitwiseshop (
                        ShopOwnerUserId, ShopifyShopId, TimeZone, CurrencyId, 
                        IsAccessTokenValid, IsShopEnabled, IsDataLoaded,
                        StartingDateForOrders, UseDefaultMargin, DefaultMargin, ProfitRealization, DateRangeDefault 
                ) VALUES (
                        @ShopOwnerUserId, @ShopifyShopId, @CurrencyId, @TimeZone,
                        @IsAccessTokenValid, @IsShopEnabled, @IsDataLoaded,
                        @StartingDateForOrders, @UseDefaultMargin,  @DefaultMargin, @ProfitRealization, @DateRangeDefault );
                SELECT LAST_INSERT_ID();";
            return _connection
                .Query<int>(query, shop)
                .First();
        }

        public void Update(PwShop shop)
        {
            var query = @"UPDATE profitwiseshop 
                        SET CurrencyId = @CurrencyId,
                            TimeZone = @TimeZone
                        WHERE PwShopId = @PwShopId";
            _connection.Execute(query, shop);
        }

        public void UpdateIsAccessTokenValid(int pwShopId, bool isAccessTokenValid)
        {
            var query = @"UPDATE profitwiseshop 
                        SET IsAccessTokenValid = @isAccessTokenValid
                        WHERE PwShopId = @pwShopId";
            _connection.Execute(query, new { pwShopId, isAccessTokenValid });
        }

        public void UpdateIsShopEnabled(int pwShopId, bool isShopEnabled)
        {
            var query = @"UPDATE profitwiseshop 
                        SET IsShopEnabled = @isShopEnabled
                        WHERE PwShopId = @pwShopId";
            _connection.Execute(query, new { pwShopId, isShopEnabled });
        }

        public void UpdateIsDataLoaded(int pwShopId, bool isDataLoaded)
        {
            var query = @"UPDATE profitwiseshop 
                        SET IsDataLoaded = @isDataLoaded
                        WHERE PwShopId = @pwShopId";
            _connection.Execute(query, new { pwShopId, isDataLoaded });
        }



        public void UpdateStartingDateForOrders(int pwShopId, DateTime startingDateForOrders)
        {
            var query = @"UPDATE profitwiseshop 
                        SET StartingDateForOrders = @startingDateForOrders
                        WHERE PwShopId = @pwShopId";
            _connection.Execute(query, new { pwShopId, startingDateForOrders });
        }

        public void UpdateDefaultMargin(int pwShopId, bool useDefaultMargin, decimal defaultMargin)
        {
            var query = @"UPDATE profitwiseshop 
                        SET UseDefaultMargin = @useDefaultMargin,
                            DefaultMargin = @defaultMargin  
                        WHERE PwShopId = @pwShopId";
            _connection.Execute(query, new { pwShopId, useDefaultMargin, defaultMargin });
        }

        public void UpdateProfitRealization(int pwShopId, int profitRealization)
        {
            var query = @"UPDATE profitwiseshop 
                        SET ProfitRealization = @profitRealization
                        WHERE PwShopId = @pwShopId";
            _connection.Execute(query, new { pwShopId, profitRealization });
        }

        public void UpdateDateRangeDefault(int pwShopId, int dateRangeDefault)
        {
            var query = @"UPDATE profitwiseshop 
                        SET DateRangeDefault = @dateRangeDefault
                        WHERE PwShopId = @pwShopId";
            _connection.Execute(query, new { pwShopId, dateRangeDefault });
        }
    }
}

