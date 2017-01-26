using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Shop;


namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwVariantRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;

        public PwVariantRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }


        public IList<PwMasterVariant> RetrieveAllMasterVariants(long? pwMasterProductId = null)
        {
            var query =
                @"SELECT t1.PwMasterVariantId, t1.PwShopId, t1.PwMasterProductId, t1.Exclude, t1.StockedDirectly, 
                        t1.CogsTypeId, t1.CogsCurrencyId, t1.CogsAmount, t1.CogsMarginPercent, t1.CogsDetail,                        
                        t2.PwVariantId, t2.PwShopId, t2.PwProductId, t2.ShopifyProductId, t2.ShopifyVariantId, 
                        t2.Sku, t2.Title, t2.LowPrice, t2.HighPrice, t2.IsActive, t2.IsPrimary
                FROM profitwisemastervariant t1
	                INNER JOIN profitwisevariant t2
		                ON t1.PwShopId = t2.PwShopId 
		                AND t1.PwMasterVariantId = t2.PwMasterVariantId
                WHERE t1.PwShopId = @PwShopId";

            dynamic rawoutput;
            if (pwMasterProductId.HasValue)
            {
                query = query + " AND t1.PwMasterProductId = ( @PwMasterProductId )";
                rawoutput = Connection.Query<dynamic>(
                    query, new { @PwShopId = this.PwShop.PwShopId, @PwMasterProductId = pwMasterProductId },
                    _connectionWrapper.Transaction);
            }
            else
            {
                rawoutput = Connection.Query<dynamic>(
                    query, new { @PwShopId = this.PwShop.PwShopId, });
            }

            var output = new List<PwMasterVariant>();

            foreach (var row in rawoutput)
            {
                var masterVariant = 
                    output.FirstOrDefault(x => x.PwMasterVariantId == row.PwMasterVariantId);

                // Bool true is stored in the dynamic object as such
                var dynamicTrue = (sbyte) 1;

                if (masterVariant == null)
                {
                    masterVariant = new PwMasterVariant();
                    masterVariant.PwMasterVariantId = row.PwMasterVariantId;
                    masterVariant.PwShopId = row.PwShopId;
                    masterVariant.PwMasterProductId = row.PwMasterProductId;
                    masterVariant.Variants = new List<PwVariant>();
                    masterVariant.Exclude = row.Exclude == dynamicTrue;
                    masterVariant.StockedDirectly = row.StockedDirectly == dynamicTrue;

                    masterVariant.CogsTypeId = row.CogsTypeId;
                    masterVariant.CogsCurrencyId = row.CogsCurrencyId;
                    masterVariant.CogsAmount = row.CogsAmount;
                    masterVariant.CogsMarginPercent = row.CogsMarginPercent;
                    masterVariant.CogsDetail = row.CogsDetail == dynamicTrue;

                    output.Add(masterVariant);
                }

                if (masterVariant.Variants.All(x => x.PwVariantId != row.PwVariantId))
                {
                    var variant = new PwVariant();
                    variant.PwVariantId = row.PwVariantId;
                    variant.PwShopId = row.PwShopId;
                    variant.PwProductId = row.PwProductId;
                    variant.PwMasterVariantId = row.PwMasterVariantId;
                    variant.ShopifyProductId = row.ShopifyProductId;
                    variant.ShopifyVariantId = row.ShopifyVariantId;
                    variant.Sku = row.Sku;
                    variant.Title = row.Title;
                    variant.LowPrice = row.LowPrice;
                    variant.HighPrice = row.HighPrice;
                    variant.IsActive = row.IsActive == dynamicTrue;
                    variant.IsPrimary = row.IsPrimary == dynamicTrue;
                    variant.ParentMasterVariant = masterVariant;
                    masterVariant.Variants.Add(variant);
                }
            }

            return output.ToList();
        }

        public long InsertMasterVariant(PwMasterVariant masterVariant)
        {
            var query =
                @"INSERT INTO profitwisemastervariant ( 
                    PwShopId, PwMasterProductId, Exclude, StockedDirectly, 
                    CogsTypeId, CogsCurrencyId, CogsAmount, CogsMarginPercent, CogsDetail ) 
                VALUES (
                    @PwShopId, @PwMasterProductId, @Exclude, @StockedDirectly,
                    @CogsTypeId, @CogsCurrencyId, @CogsAmount, @CogsMarginPercent, @CogsDetail );
                SELECT SCOPE_IDENTITY();";
            return Connection.Query<long>(query, masterVariant, _connectionWrapper.Transaction).FirstOrDefault();
        }

        public void DeleteMasterVariantByMasterVariantId(long pwMasterVariantId)
        {
            var query =
                @"DELETE FROM profitwisemastervariant " +
                @"WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;";

            Connection.Execute(
                query, new { @PwMasterVariantId = pwMasterVariantId }, _connectionWrapper.Transaction);
        }
        public void DeleteMasterVariantByProductId(long pwMasterProductId)
        {
            var query =
                @"DELETE FROM profitwisemastervariant
                WHERE PwShopId = @PwShopId AND PwMasterProductId = @pwMasterProductId;";

            Connection.Execute(query, _connectionWrapper.Transaction);
        }



        public IList<PwVariant> RetrieveAllVariants(long pwMasterVariantId)
        {
            var query = @"SELECT * FROM profitwisevariant 
                        WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;";
            return Connection
                    .Query<PwVariant>(
                        query, new { @PwShopId = this.PwShop.PwShopId, @PwMasterVariantId = pwMasterVariantId },
                        _connectionWrapper.Transaction).ToList();
        }

        public long InsertVariant(PwVariant variant)
        {
            var query = @"INSERT INTO profitwisevariant 
                            ( PwShopId, PwProductId, PwMasterVariantId, ShopifyProductId, ShopifyVariantId, SKU,  
                                Title, LowPrice, HighPrice, Inventory, IsActive, IsPrimary, IsPrimaryManual, LastUpdated ) 
                        VALUES ( @PwShopId, @PwProductId, @PwMasterVariantId, @ShopifyProductId, @ShopifyVariantId, @SKU, 
                                @Title, @LowPrice, @HighPrice, @Inventory, @IsActive, @IsPrimary, @IsPrimaryManual, @LastUpdated );
                        SELECT SCOPE_IDENTITY();";
            return Connection.Query<long>(query, variant, _connectionWrapper.Transaction).FirstOrDefault();
        }

        public void DeleteVariantByMasterVariantId(long pwMasterVariantId)
        {
            var query = @"DELETE FROM profitwisevariant
                        WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;";
            Connection.Execute(
                query, new {@PwShopId = this.PwShop.PwShopId, @PwMasterVariantId = pwMasterVariantId},
                _connectionWrapper.Transaction);
        }

        public void DeleteVariantByVariantId(long pwVariantId)
        {
            var query = @"DELETE FROM profitwisevariant
                        WHERE PwShopId = @PwShopId AND PwVariantId = @pwVariantId;";
            Connection.Execute(
                query, new { @PwShopId = this.PwShop.PwShopId, PwVariantId = pwVariantId },
                _connectionWrapper.Transaction);
        }

        public void UpdateVariantIsActive(PwVariant variant)
        {
            var query = @"UPDATE profitwisevariant SET IsActive = @IsActive
                            WHERE PwShopId = @PwShopId 
                            AND PwVariantId = @pwVariantId;";

            Connection.Execute(
                query, new { @PwShopId = this.PwShop.PwShopId, PwVariantId = variant.PwVariantId, IsActive = variant.IsActive,}, 
                _connectionWrapper.Transaction);
        }

        public void UpdateVariantIsPrimary(PwVariant variant)
        {
            var query = @"UPDATE profitwisevariant
                            SET IsPrimary = @IsPrimary
                            WHERE PwShopId = @PwShopId AND PwVariantId = @PwVariantId";
            Connection.Execute(query, variant, _connectionWrapper.Transaction);
        }

        public void UpdateVariantPriceAndInventory(
                    long pwVariantId, decimal lowPrice, decimal highPrice, int? inventory)
        {
            var query = @"UPDATE profitwisevariant 
                        SET LowPrice = @lowPrice, HighPrice = @highPrice, Inventory = @inventory
                        WHERE PwShopId = @PwShopId
                        AND PwVariantId = @pwVariantId;";

            Connection.Execute(query,
                    new
                    {
                        @PwShopId = this.PwShop.PwShopId,
                        PwVariantId = pwVariantId,
                        LowPrice = lowPrice,
                        HighPrice = highPrice,
                        Inventory = inventory,
                    },
                    _connectionWrapper.Transaction);
        }

        public void DeleteOrphanedMasterVariants()
        {
            var query = @"DELETE FROM profitwisemastervariant 
                        WHERE PwShopId = @PwShopId AND PwMasterVariantId NOT IN 
                            ( SELECT PwMasterVariantId FROM profitwisevariant );";
            Connection.Execute(query, new { @PwShopId = this.PwShop.PwShopId, },
                            _connectionWrapper.Transaction);
        }
    }
}

