﻿using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopIdRequired))]
    public class PwProductRepository : IShopIdFilter
    {
        private readonly MySqlConnection _connection;

        public int? PwShopId { get; set; }

        public PwProductRepository(MySqlConnection connection)
        {
            _connection = connection;
        }


        //
        // TODO => add paging and filtering
        //
        public IList<PwMasterProduct> RetrieveAllMasterProducts()
        {
            var products = RetrieveAllProducts();

            var masterProducts = 
                products
                    .Select(x => x.PwMasterProductId)
                    .Distinct()    
                    .Select(masterProductId => new PwMasterProduct
                    {
                        PwShopId = this.PwShopId.Value,
                        PwMasterProductId = masterProductId,
                        Products = products.Where(product => masterProductId == product.PwMasterProductId).ToList()
                    }).ToList();

            return masterProducts;
        }

        public long InsertMasterProduct(PwMasterProduct masterProduct)
        {
            var query =
                    @"INSERT INTO profitwisemasterproduct ( PwShopId )
                    VALUES ( @PwShopId );
                    SELECT LAST_INSERT_ID();";

            return _connection.Query<long>(query, masterProduct).FirstOrDefault();
        }

        public void DeleteMasterProduct(PwMasterProduct masterProduct)
        {
            var query =
                @"DELETE FROM profitwisemasterproduct " +
                @"WHERE PwShopId = @PwShopId AND PwMasterProductId = @PwMasterProductId;";

            _connection.Execute(query);
        }



        public IList<PwProduct> RetrieveAllProducts()
        {
            var query = @"SELECT * FROM profitwiseproduct WHERE PwShopId = @PwShopId";
            return _connection
                    .Query<PwProduct>(query, new { @ShopId = this.PwShopId } ).ToList();
        }

        public PwProduct RetrieveProduct(long pwProductId)
        {
            var query = @"SELECT * FROM profitwiseproduct WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId;";
            return _connection
                    .Query<PwProduct>(query, new { @PwShopId = this.PwShopId, @PwProductId = pwProductId })
                    .FirstOrDefault();
        }

        public long InsertProduct(PwProduct product)
        {
            var query = @"INSERT INTO profitwiseproduct 
                            ( PwShopId, PwProductId, PwMasterProductId, ShopifyProductId, Title, Vendor, ProductType, Active, Primary, Tags ) 
                        VALUES ( @PwShopId, @PwProductId, @PwMasterProductId, @ShopifyProductId, @Title, @Vendor, @ProductType, @Active, @Primary, @Tags );
                        SELECT LAST_INSERT_ID();";
            return _connection.Query<long>(query, product).FirstOrDefault();
        }

        public void UpdateProduct(PwProduct product)
        {
            var query = @"UPDATE profitwiseproduct
                            SET Active = @Active,
                                Primary = @Primary
                            WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId";
            _connection.Execute(query, product);
        }
    }
}

