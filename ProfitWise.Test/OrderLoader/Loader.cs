using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Model;
using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.Json;
using System.Linq;

namespace ProfitWise.Test.OrderLoader
{
    [TestClass]
    public class Loader
    {
        [TestMethod]
        public void CreateOrders()
        {
            var container = ProfitWise.Batch.AutofacRegistration.Build();

            //Use the userId for the "Super Great Deals" test store

            var userId = "e507de54-d56d-41af-ba83-5ffa28b753da";

            using (var scope = container.BeginLifetimeScope())
            {
                // Step #1 - call Shopify and get our Product catalog
                var factory = scope.Resolve<ApiRepositoryFactory>();
                var credentialService = scope.Resolve<IShopifyCredentialService>();

                var claims = credentialService.Retrieve(userId);
                var credentials = new ShopifyCredentials()
                {
                    ShopOwnerUserId = claims.ShopOwnerUserId,
                    ShopDomain = claims.ShopDomain,
                    AccessToken = claims.AccessToken,
                };

                var productApiRepository = factory.MakeProductApiRepository(credentials);
                var orderApiRepository = factory.MakeOrderApiRepository(credentials);

                var filter = new ProductFilter();
                var products = productApiRepository.Retrieve(filter);
                var numProducts = products.Count;

                var numOrders = 7195;

                //Create orders
                for (int i = 1; i < numOrders + 1; i = i + 1)
                {

                    var randomDate = RandomDay();

                    Random rnd = new Random();
                    //Set number of line items to random # 1-10
                    int numLineItems = rnd.Next(1, 10);

                    var line_items = new List<object>();
                    var orderAmount = 0.0m;

                    //create 1-10 line items
                    for (int li = 1; li < numLineItems + 1; li = li + 1)
                    {
                        //Set line item quantity to random # 1-10
                        int quantity = rnd.Next(1, 10);

                        //Pick random product index
                        var productNum = rnd.Next(0, numProducts - 1);
                        var product = products[productNum];

                        //Pick random variant index from selected product
                        var variantNum = rnd.Next(0, product.Variants.Count - 1);
                        var variant = product.Variants[variantNum];



                        line_items.Add(new
                        {
                            product_id = product.Id,
                            title = product.Title,
                            variant_title = variant.Title,
                            variant_id = variant.Id,
                            vendor = product.Vendor,
                            sku = variant.Sku,
                            price = variant.Price,
                            quantity = quantity
                        });

                        orderAmount = orderAmount + (variant.Price * quantity);

                    }


                    var order = new
                    {
                        order = new
                        {
                            line_items = line_items,
                            customer = new
                            {
                                id = 5341483533,
                                email = "johndoe@fakedomain.com"
                            },
                            financial_status = "paid",
                            transactions = new[]
                                 {
                                    new {
                                        test = "true",
                                        kind = "sale",
                                        status = "success",
                                        amount = orderAmount
                                    }
                                 },
                            total_tax = "0",
                            currency = "USD",
                            processed_at = randomDate
                        }
                    };

                    var json = order.SerializeToJson();
                    System.IO.File.WriteAllText(@"C:\Dev\ProfitWise\Logs\JSonDump\jsondump" + i + ".txt", json);
                    orderApiRepository.Insert(json);


                }

            }

        }

        private Random gen = new Random();
        DateTime RandomDay()
        {
            DateTime start = new DateTime(2014, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(gen.Next(range));
        }

    }

}
