using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Billing;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories.Multitenant
{
    [Intercept(typeof(ShopRequired))]
    public class BillingRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        private readonly ConnectionWrapper _connectionWrapper;
        
        public BillingRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }
        
        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }


        public IList<PwRecurringCharge> RetrieveAll()
        {
            var query = @"SELECT * FROM recurringcharge(@PwShopId)";
            return
                _connectionWrapper
                    .Query<PwRecurringCharge>(query, new {PwShopId = PwShop.PwShopId})
                    .ToList();
        }

        public PwRecurringCharge RetrieveCurrent()
        {
            var query = @"SELECT * FROM recurringcharge(@PwShopId) WHERE IsPrimary = 1";
            return
                _connectionWrapper
                    .Query<PwRecurringCharge>(query, new { PwShopId = PwShop.PwShopId })
                    .FirstOrDefault();
        }
        
        public long RetrieveNextKey()
        {
            var query = @"SELECT ISNULL(MAX(PwChargeId), 0) + 1 FROM recurringcharge(@PwShopId)";
            return
                _connectionWrapper
                    .Query<long>(query, new { PwShopId = PwShop.PwShopId })
                    .First();
        }


        public void Insert(PwRecurringCharge charge)
        {
            var query = @"INSERT INTO recurringcharge(@PwShopId) VALUES (
                            @PwShopId, @PwChargeId, @ShopifyRecurringChargeId, @ConfirmationUrl, 
                            @LastStatus, @IsPrimary, getdate(), getdate() );";
            _connectionWrapper.Execute(query, charge);
        }

        public void Update(PwRecurringCharge state)
        {
            var query = @"UPDATE recurringcharge(@PwShopId) 
                        SET ConfirmationUrl	= @ConfirmationUrl,
                            LastStatus = @LastStatus, 
                            LastUpdated = getdate(),
                            LastJson = @LastJson
                        WHERE PwChargeId = PwChargeId;";
            _connectionWrapper.Execute(query, state); 
        }

        public void UpdatePrimary(long primaryChargeId)
        {
            var query =
                @"UPDATE recurringcharge(@PwShopId) SET IsPrimary = 1 WHERE PwChargeId = @activeChargeId;
                UPDATE recurringcharge(@PwShopId) SET IsPrimary = 0 WHERE PwChargeId <> @activeChargeId;";
            _connectionWrapper.Execute(query, primaryChargeId);
        }

        public void ClearPrimary()
        {
            var query = @"UPDATE recurringcharge(@PwShopId) SET IsPrimary = 0;";
            _connectionWrapper.Execute(query);
        }
    }
}

