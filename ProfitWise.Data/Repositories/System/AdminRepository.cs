using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Admin;

namespace ProfitWise.Data.Repositories.System
{
    public class AdminRepository
    {
        private readonly ConnectionWrapper _connectionWrapper;

        public AdminRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        string usersQuery =
                @"SELECT t1.Id AS UserId, UserName, Email, t4.TimeZone, t4.Domain, t4.CurrencyId, 
                        t4.PwShopId, t4.IsAccessTokenValid, t4.IsProfitWiseInstalled, t4.IsDataLoaded, 
                        t5.ProductsLastUpdated, t5.OrderDatasetStart, t5.OrderDatasetEnd,
                        t4.TempFreeTrialOverride, t6.LastStatus AS LastBillingStatus,
                        t5.InitialRefreshJobId, t5.RoutineRefreshJobId
                FROM AspNetUsers t1 
	                INNER JOIN AspNetUserRoles t2 ON t1.Id = t2.UserId
	                INNER JOIN AspNetRoles t3 ON t2.RoleId = t3.Id AND t3.Name = 'USER'
	                INNER JOIN profitwiseshop t4 ON t1.Id = t4.ShopOwnerUserId
                    LEFT JOIN profitwisebatchstate t5 on t4.PwShopId = t5.PwShopId
                    LEFT JOIN profitwiserecurringcharge t6 ON t4.PwShopId = t6.PwShopId AND t6.IsPrimary = 1 ";

        public IList<ProfitWiseUser> RetrieveUsers()
        {
            return _connectionWrapper.Query<ProfitWiseUser>(usersQuery, new {}).ToList();
        }

        public ProfitWiseUser RetrieveUser(string userId)
        {
            var query = usersQuery + " WHERE t1.Id = @userId;";
            return _connectionWrapper.Query<ProfitWiseUser>(query, new { userId }).FirstOrDefault();
        }

        public string RetrieveUserIdByUserName(string userName)
        {
            var query = "SELECT Id FROM AspNetUsers WHERE UserName = @userName";
            return _connectionWrapper.Query<string>(query, new {userName}).FirstOrDefault();
        }
    }
}

