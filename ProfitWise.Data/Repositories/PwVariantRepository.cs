using System;
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

        public IList<PwMasterVariant> 
                RetrieveMasterVariants(long? pwMasterProductId = null, long? pwMasterVariantId = null)
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

            if (pwMasterProductId.HasValue)
            {
                query = query + " AND t1.PwMasterProductId = @PwMasterProductId ";
            }
            if (pwMasterVariantId.HasValue)
            {
                query = query + " AND t1.PwMasterVariantId = @PwMasterVariantId ";
            }
            
            dynamic rawoutput = 
                Connection.Query<dynamic>(query, 
                    new { PwShop.PwShopId, PwMasterProductId = pwMasterProductId, PwMasterVariantId = pwMasterVariantId, },
                    _connectionWrapper.Transaction);

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

        public PwMasterVariant InsertCopy(long sourceMasterVariantId, long targetMasterProductId)
        {
            var query =
                @"INSERT INTO profitwisemastervariant ( 
                    PwShopId, PwMasterProductId, Exclude, StockedDirectly, 
                    CogsTypeId, CogsCurrencyId, CogsAmount, CogsMarginPercent, CogsDetail ) 
                SELECT PwShopId, @targetMasterProductId, Exclude, StockedDirectly,
                    CogsTypeId, CogsCurrencyId, CogsAmount, CogsMarginPercent, CogsDetail
                FROM profitwisemastervariant 
                WHERE PwShopId = @PwShopId AND PwMasterVariantId = @sourceMasterVariantId;
                SELECT * FROM profitwisemastervariant
                WHERE PwShopId = @PwShopId AND PwMasterVariantId IN ( SELECT SCOPE_IDENTITY() );";

            return Connection.Query<PwMasterVariant>(
                query, new { PwShop.PwShopId, sourceMasterVariantId, targetMasterProductId },
                _connectionWrapper.Transaction).First();
        }

        public void DeleteMasterVariant(long pwMasterVariantId)
        {
            var query =
                @"DELETE FROM profitwisemastervariantcogscalc
                WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;
                
                DELETE FROM profitwisemastervariantcogsdetail
                WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;
                
                DELETE FROM profitwisemastervariant 
                WHERE PwShopId = @PwShopId AND PwMasterVariantId = @pwMasterVariantId;";

            Connection.Execute(
                query, new { this.PwShop.PwShopId, @PwMasterVariantId = pwMasterVariantId }, 
                _connectionWrapper.Transaction);
        }

        [Obsolete]
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

        public IList<PwVariant> RetrieveVariantsForMasterProduct(long pwMasterProductId)
        {
            var query = @"SELECT t1.* FROM profitwisevariant t1
                            INNER JOIN profitwisemastervariant t2
                                ON t1.PwMasterVariantId = t2.PwMasterVariantId 
                        WHERE t2.PwShopId = @PwShopId 
                        AND t2.PwMasterProductId = @pwMasterProductId";

            return Connection
                    .Query<PwVariant>(
                        query, new { PwShop.PwShopId, pwMasterProductId },
                        _connectionWrapper.Transaction).ToList();
        }

        public IList<PwVariant> RetrieveVariantsForMasterVariant(long pwMasterVariantId)
        {
            var query = @"SELECT t1.* FROM profitwisevariant t1
                        WHERE t1.PwShopId = @PwShopId AND t1.PwMasterVariantId = @pwMasterVariantId";

            return Connection
                    .Query<PwVariant>(
                        query, new { PwShop.PwShopId, pwMasterVariantId },
                        _connectionWrapper.Transaction).ToList();
        }

        public long RetrieveMasterVariantId(long pwVariantId)
        {
            var query = @"SELECT PwMasterVariantId FROM profitwisevariant 
                        WHERE PwShopId = @PwShopId AND pwVariantId = @pwVariantId";

            return Connection.Query<long>(
                query, new { PwShop.PwShopId, pwVariantId }, _connectionWrapper.Transaction).First();
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
                            SET IsPrimary = @IsPrimary, IsPrimaryManual = @IsPrimaryManual
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

        public void UpdateVariantsMasterVariant(PwVariant variant)
        {
            var query = @"UPDATE profitwisevariant
                            SET PwMasterVariantId = @PwMasterVariantId
                            WHERE PwShopId = @PwShopId AND PwVariantId = @PwVariantId";
            Connection.Execute(query, variant, _connectionWrapper.Transaction);
        }


        public void DeleteChildlessMasterVariants()
        {
            var query =
                    @"DELETE FROM profitwisemastervariantcogsdetail
                    WHERE PwShopId = @PwShopId AND PwMasterVariantId NOT IN 
                        ( SELECT PwMasterVariantId FROM profitwisevariant );
                    
                    DELETE FROM profitwisemastervariantcogscalc
                    WHERE PwShopId = @PwShopId AND PwMasterVariantId NOT IN 
                        ( SELECT PwMasterVariantId FROM profitwisevariant );

                    DELETE FROM profitwisemastervariant 
                    WHERE PwShopId = @PwShopId AND PwMasterVariantId NOT IN 
                        ( SELECT PwMasterVariantId FROM profitwisevariant );";

            Connection.Execute(query, new { @PwShopId = this.PwShop.PwShopId, }, _connectionWrapper.Transaction);
        }
    }
}

