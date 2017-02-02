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

namespace ProfitWise.Test.OrderLoader
{
    [TestClass]
    public class Loader
    {
        [TestMethod]
        public void Execute()
        {
            var container = ProfitWise.Batch.AutofacRegistration.Build();

            var userId = "7ab46e72-3b1c-4db1-88d4-f8a6b8f3e57a";
            // Jeremy: var userId = "c5223b47-aa25-4261-9639-68d749bdc38b";

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

                // Step #2 - prepare a plan to upload random Orders

                var numOrders = 5000;

                var line_items = new List<object>();

                // for each item I'm creating....
                line_items.Add(new
                {
                    title = "Product title",
                    price = 74.99,
                    grams = "1300",
                    quantity = 3
                });

                var order = new
                {
                    order = new
                    {
                        line_items = line_items,
                        customer = new {
                            id = 5341483533
                        },
                        financial_status = "paid",
                        transactions = new []
                        {
			                new {
				                test = "true",
				                kind = "sale",
				                status = "success",
				                amount = 238.47m
                            }
		                 },
                         total_tax = "0",
                         currency = "USD"
                    }
                };

                var json = order.SerializeToJson();
                System.IO.File.WriteAllText(@"C:\Dev\ProfitWise\Logs\jsondump.txt", json);
             //   orderApiRepository.Insert(json);

                return;

                //Create 5000 orders
                for (int i = 1; i < numOrders; i = i + 1)
                {
                    var randomDate = RandomDay();

                    Random rnd = new Random();
                    //Set number of line items to random # 1-10
                    int numLineItems = rnd.Next(1, 10);

                    //create 1-10 line items
                    for (int li = 1; li < numLineItems; li = li + 1)
                    {
                        //Set line item quantity to random # 1-10
                        int quantity = rnd.Next(1, 10);

                        //Pick random product index
                        var productNum = rnd.Next(0, numProducts-1);
                        var product = products[productNum];

                        //Pick random variant index from selected product
                        var variantNum = rnd.Next(0, product.Variants.Count-1);
                        var variant = product.Variants[variantNum];

                        //Set values to be inserted into order line item
                        var variantId = variant.Id;
                        var price = variant.Price;
                        var sku = variant.Sku;
                        var title = variant.Title;
                    }
                }


                // Step #3 - Insert Orders into Shopify API
                

                //This is a sample of what we need to generate:
                //





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
