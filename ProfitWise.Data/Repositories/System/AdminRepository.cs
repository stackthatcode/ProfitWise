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

        public IList<ProfitWiseUser> RetrieveUsers()
        {
            var query =
                @"SELECT t1.Id AS UserId, UserName, Email, t4.TimeZone, t4.Domain, t4.CurrencyId, t4.PwShopId
                FROM AspNetUsers t1 
	                INNER JOIN AspNetUserRoles t2 ON t1.Id = t2.UserId
	                INNER JOIN AspNetRoles t3 ON t2.RoleId = t3.Id AND t3.Name = 'USER'
	                INNER JOIN profitwiseshop t4 ON t1.Id = t4.ShopOwnerUserId;";

            return _connectionWrapper.Query<ProfitWiseUser>(query, new {}).ToList();
        }
    }
}

