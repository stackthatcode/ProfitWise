﻿using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;
using Push.Foundation.Utilities.General;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopIdRequired))]
    public class PwVariantRepository : IShopIdFilter
    {
        private readonly MySqlConnection _connection;

        public int? PwShopId { get; set; }

        public PwVariantRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public IList<PwMasterVariant> RetrieveAllMasterVariants(long pwProductId)
        {
            return RetrieveAllMasterVariants(new List<long> {pwProductId});
        }

        //
        // TODO => add paging and filtering
        //
        public IList<PwMasterVariant> RetrieveAllMasterVariants(IList<long> pwProductIdList)
        {
            var query = @"SELECT * FROM profitwisemastervariant 
                        WHERE PwShopId = @PwShopId
                        AND PwProductId IN ( @PwProductIdList )";
            return _connection
                    .Query<PwMasterVariant>(
                            query, 
                            new { @PwShopId = this.PwShopId,
                                @PwProductIdList = pwProductIdList.ToCommaSeparatedList() }).ToList();
        }

        public long InsertMasterVariant(PwMasterVariant masterVariant)
        {
            var query =
                    @"INSERT INTO profitwisemastervariant 
                        ( PwShopId, PwMasterVariantId, PwProductId, Exclude, StockedDirectly ) 
                    VALUES ( @PwShopId, @PwMasterVariantId, @PwProductId, @Exclude, @StockedDirectly );
                    SELECT LAST_INSERT_ID();";

            return _connection.Query<long>(query, masterVariant).FirstOrDefault();
        }

        public void DeleteMasterVariantByMasterVariantId(long pwMasterVariantId)
        {
            var query =
                @"DELETE FROM profitwisemastervariant " +
                @"WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;";

            _connection.Execute(query, new { @PwMasterVariantId = pwMasterVariantId });
        }
        public void DeleteMasterVariantByProductId(long pwProductId)
        {
            var query =
                @"DELETE FROM profitwisemastervariant
                WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId;";

            _connection.Execute(query);
        }



        public IList<PwVariant> RetrieveAllVariants(long pwMasterVariantId)
        {
            var query = @"SELECT * FROM profitwisevariant 
                        WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;";
            return _connection
                    .Query<PwVariant>(query, new { @PwShopId = this.PwShopId, @PwMasterVariantId = pwMasterVariantId } ).ToList();
        }

        public long InsertVariant(PwVariant variant)
        {
            var query = @"INSERT INTO profitwisevariant 
                            ( PwShopId, PwVariantId, PwMasterVariantId, ShopifyVariantId, Active, Primary ) 
                        VALUES ( @PwShopId, @PwProductId, @PwMasterVariantId, @ShopifyVariantId, @Active, @Primary );
                        SELECT LAST_INSERT_ID();";
            return _connection.Query<long>(query, variant).FirstOrDefault();
        }

        public void DeleteVariantByMasterVariantId(long pwMasterVariantId)
        {
            var query = @"DELETE FROM profitwisevariant
                        WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;";
            _connection.Execute(query, new {@PwShopId = this.PwShopId, @PwMasterVariantId = pwMasterVariantId});
        }

        public void DeleteVariantByVariantId(long pwVariantId)
        {
            var query = @"DELETE FROM profitwisevariant
                        WHERE PwShopId = @PwShopId AND PwVariantId = @pwVariantId;";
            _connection.Execute(query, new { @PwShopId = this.PwShopId, @PwMasterVariantId = pwVariantId });
        }
    }
}


