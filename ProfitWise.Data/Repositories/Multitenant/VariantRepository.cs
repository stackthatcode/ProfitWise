using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories.Multitenant
{
    [Intercept(typeof(ShopRequired))]
    public class VariantRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;

        
        public VariantRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }

        public IList<PwMasterVariant> 
                RetrieveMasterVariants(long? pwMasterProductId = null, long? pwMasterVariantId = null)
        {
            var query =
                @"SELECT t1.PwMasterVariantId, t1.PwShopId, t1.PwMasterProductId, t1.Exclude, t1.StockedDirectly, 
                        t1.CogsTypeId, t1.CogsCurrencyId, t1.CogsAmount, t1.CogsMarginPercent, t1.CogsDetail,                        
                        t2.PwVariantId, t2.PwShopId, t2.PwProductId, t2.ShopifyProductId, t2.ShopifyVariantId, 
                        t2.Sku, t2.Title, t2.LowPrice, t2.HighPrice, t2.IsActive, t2.IsPrimary
                FROM mastervariant(@PwShopId) t1
	                INNER JOIN variant(@PwShopId) t2
		                ON t1.PwMasterVariantId = t2.PwMasterVariantId
                WHERE t1.PwShopId = @PwShopId ";

            if (pwMasterProductId.HasValue)
            {
                query = query + " AND t1.PwMasterProductId = @PwMasterProductId ";
            }
            if (pwMasterVariantId.HasValue)
            {
                query = query + " AND t1.PwMasterVariantId = @PwMasterVariantId ";
            }

            var data = new
            {
                PwShop.PwShopId,
                PwMasterProductId = pwMasterProductId,
                PwMasterVariantId = pwMasterVariantId,
            };

            dynamic rawoutput =  _connectionWrapper.Query<dynamic>(query, data);
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
                @"INSERT INTO mastervariant(@PwShopId) ( 
                    PwShopId, PwMasterProductId, Exclude, StockedDirectly, 
                    CogsTypeId, CogsCurrencyId, CogsAmount, CogsMarginPercent, CogsDetail ) 
                VALUES (
                    @PwShopId, @PwMasterProductId, @Exclude, @StockedDirectly,
                    @CogsTypeId, @CogsCurrencyId, @CogsAmount, @CogsMarginPercent, @CogsDetail );
                SELECT SCOPE_IDENTITY();";
            return _connectionWrapper.Query<long>(query, masterVariant).FirstOrDefault();
        }

        public PwMasterVariant InsertCopy(long sourceMasterVariantId, long targetMasterProductId)
        {
            var query =
                @"INSERT INTO mastervariant(@PwShopId) ( 
                    PwShopId, PwMasterProductId, Exclude, StockedDirectly, 
                    CogsTypeId, CogsCurrencyId, CogsAmount, CogsMarginPercent, CogsDetail ) 
                SELECT PwShopId, @targetMasterProductId, Exclude, StockedDirectly,
                    CogsTypeId, CogsCurrencyId, CogsAmount, CogsMarginPercent, CogsDetail
                FROM mastervariant(@PwShopId) 
                WHERE PwMasterVariantId = @sourceMasterVariantId;
                
                SELECT * FROM mastervariant(@PwShopId)
                WHERE PwMasterVariantId IN ( SELECT SCOPE_IDENTITY() );";

            return _connectionWrapper.Query<PwMasterVariant>(
                query, new { PwShop.PwShopId, sourceMasterVariantId, targetMasterProductId }).First();
        }

        public void DeleteMasterVariant(long pwMasterVariantId)
        {
            var query =
                @"DELETE FROM mastervariantcogscalc(@PwShopId) WHERE PwMasterVariantId = @pwMasterVariantId;
                
                DELETE FROM mastervariantcogsdetail(@PwShopId) WHERE PwMasterVariantId = @pwMasterVariantId;
                
                DELETE FROM mastervariant(@PwShopId) WHERE PwMasterVariantId = @pwMasterVariantId;";

            _connectionWrapper.Execute(
                query, new { this.PwShop.PwShopId, @PwMasterVariantId = pwMasterVariantId });
        }        

        public IList<PwVariant> RetrieveAllVariants(long pwMasterVariantId)
        {
            var query = @"SELECT * FROM variant(@PwShopId) WHERE PwMasterVariantId = @pwMasterVariantId;";
            return _connectionWrapper.Query<PwVariant>(
                        query, new { PwShop.PwShopId, @PwMasterVariantId = pwMasterVariantId }).ToList();
        }

        public IList<PwVariant> RetrieveVariantsForMasterProduct(long pwMasterProductId)
        {
            var query = @"SELECT t1.* FROM variant(@PwShopId) t1
                            INNER JOIN mastervariant(@PwShopId) t2
                                ON t1.PwMasterVariantId = t2.PwMasterVariantId 
                        WHERE t2.PwMasterProductId = @pwMasterProductId";

            return _connectionWrapper
                    .Query<PwVariant>(query, new { PwShop.PwShopId, pwMasterProductId }).ToList();
        }

        public IList<long> RetrieveMasterVariantIdsForMasterProduct(long pwMasterProductId)
        {
            var query = @"SELECT PwMasterVariantId 
                        FROM mastervariant(@PwShopId) t2
                        WHERE t2.PwMasterProductId = @pwMasterProductId";

            return _connectionWrapper.Query<long>(
                query, new { PwShop.PwShopId, pwMasterProductId }).ToList();
        }


        public IList<PwVariant> RetrieveVariantsForMasterVariant(long pwMasterVariantId)
        {
            var query = @"SELECT t1.* FROM variant(@PwShopId) t1 WHERE t1.PwMasterVariantId = @pwMasterVariantId";

            return _connectionWrapper
                    .Query<PwVariant>(query, new { PwShop.PwShopId, pwMasterVariantId }).ToList();
        }

        public long RetrieveMasterVariantId(long pwVariantId)
        {
            var query = @"SELECT PwMasterVariantId FROM variant(@PwShopId) WHERE PwVariantId = @pwVariantId";
            return _connectionWrapper.Query<long>(query, new { PwShop.PwShopId, pwVariantId }).First();
        }

        public long InsertVariant(PwVariant variant)
        {
            var query = @"INSERT INTO variant(@PwShopId) 
                            ( PwShopId, PwProductId, PwMasterVariantId, ShopifyProductId, ShopifyVariantId, SKU,  
                                Title, LowPrice, HighPrice, Inventory, IsActive, IsPrimary, IsPrimaryManual, LastUpdated ) 
                        VALUES ( @PwShopId, @PwProductId, @PwMasterVariantId, @ShopifyProductId, @ShopifyVariantId, @SKU, 
                                @Title, @LowPrice, @HighPrice, @Inventory, @IsActive, @IsPrimary, @IsPrimaryManual, @LastUpdated );
                        SELECT SCOPE_IDENTITY();";
            return _connectionWrapper.Query<long>(query, variant).FirstOrDefault();
        }

        public void DeleteVariantByMasterVariantId(long pwMasterVariantId)
        {
            var query = @"DELETE FROM variant(@PwShopId) WHERE PwMasterVariantId = @pwMasterVariantId;";
            _connectionWrapper.Execute(
                query, new { PwShop.PwShopId, @PwMasterVariantId = pwMasterVariantId});
        }

        public void DeleteVariantByVariantId(long pwVariantId)
        {
            var query = @"DELETE FROM variant(@PwShopId) WHERE PwVariantId = @pwVariantId;";
            _connectionWrapper.Execute(query, new { PwShop.PwShopId, PwVariantId = pwVariantId });
        }

        public void UpdateVariantIsActive(PwVariant variant)
        {
            var query = @"UPDATE variant(@PwShopId) SET IsActive = @IsActive WHERE PwVariantId = @pwVariantId;";

            _connectionWrapper.Execute(query, new { PwShop.PwShopId, variant.PwVariantId, variant.IsActive,});
        }

        public void UpdateVariantIsPrimary(PwVariant variant)
        {
            var query = @"UPDATE variant(@PwShopId)
                            SET IsPrimary = @IsPrimary, IsPrimaryManual = @IsPrimaryManual
                            WHERE PwVariantId = @PwVariantId";
            _connectionWrapper.Execute(query, variant);
        }

        public void UpdateVariant(
                long pwVariantId, decimal lowPrice, decimal highPrice, string sku, int? inventory)
        {
            var query = @"UPDATE variant(@PwShopId) 
                        SET LowPrice = @lowPrice, 
                            HighPrice = @highPrice, 
                            Sku = @sku,
                            Inventory = @inventory
                        WHERE PwVariantId = @pwVariantId;";

            _connectionWrapper.Execute(
                query, new { PwShop.PwShopId, pwVariantId, lowPrice, highPrice, inventory, sku, });
        }

        public void UpdateVariantsMasterVariant(PwVariant variant)
        {
            var query = @"UPDATE variant(@PwShopId)
                            SET PwMasterVariantId = @PwMasterVariantId
                            WHERE PwVariantId = @PwVariantId";
            _connectionWrapper.Execute(query, variant);
        }

        public void DeleteChildlessMasterVariants()
        {
            var query =
                    @"DELETE FROM mastervariantcogsdetail(@PwShopId)
                    WHERE PwMasterVariantId NOT IN 
                        ( SELECT PwMasterVariantId FROM variant(@PwShopId) );
                    
                    DELETE FROM mastervariantcogscalc(@PwShopId)
                    WHERE PwMasterVariantId NOT IN 
                        ( SELECT PwMasterVariantId FROM variant(@PwShopId) );

                    DELETE FROM mastervariant(@PwShopId) 
                    WHERE PwMasterVariantId NOT IN 
                        ( SELECT PwMasterVariantId FROM variant(@PwShopId) );";

            _connectionWrapper.Execute(query, new {PwShop.PwShopId,});
        }
    }
}

