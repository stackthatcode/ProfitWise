using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;


namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwVariantRepository : IShopFilter
    {
        private readonly MySqlConnection _connection;
        public PwShop PwShop { get; set; }


        public PwVariantRepository(MySqlConnection connection)
        {
            _connection = connection;
        }



        //
        // TODO => add paging and filtering
        //
        public IList<PwMasterVariant> RetrieveAllMasterVariants(long? pwMasterProductId = null)
        {
            var query =
                @"SELECT t1.*, t2.* FROM profitwisemastervariant t1
                    INNER JOIN profitwisevariant t2 
                ON t1.PwShopId = t2.PwShopId AND t1.PwMasterVariantId = t2.PwMasterVariantId
                WHERE t1.PwShopId = @PwShopId";

            if (pwMasterProductId.HasValue)
            {
                query = query + " AND t1.PwMasterProductId = ( @PwMasterProductId )";
            }

            var masterVariantOutputList = new List<PwMasterVariant>();

            Func<PwMasterVariant, PwVariant, PwMasterVariant>
                buildFunc
                    = (mv, v) =>
                        {
                            if (masterVariantOutputList.All(x => x.PwMasterVariantId != mv.PwMasterVariantId))
                            {
                                masterVariantOutputList.Add(mv);
                            }
                            v.ParentMasterVariant = mv;
                            mv.Variants.Add(v);
                            return mv;
                        };

            if (pwMasterProductId.HasValue)
            {
                _connection.Query(
                        query, buildFunc, 
                        new { @PwShopId = this.PwShop.PwShopId, @PwMasterProductId = pwMasterProductId },
                        splitOn: "PwMasterVariantId"
                    ).AsQueryable();
            }
            else
            {
                _connection.Query(
                        query, buildFunc, 
                        new { @PwShopId = this.PwShop.PwShopId, },
                        splitOn: "PwMasterVariantId"
                    ).AsQueryable();
            }

            return masterVariantOutputList;
        }

        public long InsertMasterVariant(PwMasterVariant masterVariant)
        {
            var query =
                    @"INSERT INTO profitwisemastervariant 
                        ( PwShopId, PwMasterProductId, Exclude, StockedDirectly ) 
                    VALUES ( @PwShopId, @PwMasterProductId, @Exclude, @StockedDirectly );
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
        public void DeleteMasterVariantByProductId(long pwMasterProductId)
        {
            var query =
                @"DELETE FROM profitwisemastervariant
                WHERE PwShopId = @PwShopId AND PwMasterProductId = @pwMasterProductId;";

            _connection.Execute(query);
        }



        public IList<PwVariant> RetrieveAllVariants(long pwMasterVariantId)
        {
            var query = @"SELECT * FROM profitwisevariant 
                        WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;";
            return _connection
                    .Query<PwVariant>(query, new { @PwShopId = this.PwShop.PwShopId, @PwMasterVariantId = pwMasterVariantId } ).ToList();
        }

        public long InsertVariant(PwVariant variant)
        {
            var query = @"INSERT INTO profitwisevariant 
                            ( PwShopId, PwProductId, PwMasterVariantId, ShopifyVariantId, SKU, Title, IsActive, IsPrimary ) 
                        VALUES ( @PwShopId, @PwProductId, @PwMasterVariantId, @ShopifyVariantId, @SKU, @Title, @IsActive, @IsPrimary );
                        SELECT LAST_INSERT_ID();";
            return _connection.Query<long>(query, variant).FirstOrDefault();
        }

        public void DeleteVariantByMasterVariantId(long pwMasterVariantId)
        {
            var query = @"DELETE FROM profitwisevariant
                        WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;";
            _connection.Execute(query, new {@PwShopId = this.PwShop.PwShopId, @PwMasterVariantId = pwMasterVariantId});
        }

        public void DeleteVariantByVariantId(long pwVariantId)
        {
            var query = @"DELETE FROM profitwisevariant
                        WHERE PwShopId = @PwShopId AND PwVariantId = @pwVariantId;";
            _connection.Execute(query, new { @PwShopId = this.PwShop.PwShopId, @PwMasterVariantId = pwVariantId });
        }
    }
}

