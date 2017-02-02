using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Model;
using System;

namespace ProfitWise.Test.OrderLoader
{
    [TestClass]
    public class Loader
    {
        [TestMethod]
        public void Execute()
        {
            var container = ProfitWise.Batch.AutofacRegistration.Build();

            // Aleks: var userId = "7ab46e72-3b1c-4db1-88d4-f8a6b8f3e57a";
            var userId = "c5223b47-aa25-4261-9639-68d749bdc38b";

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

                var filter = new ProductFilter();
                var products = productApiRepository.Retrieve(filter);
                var numProducts = products.Count;

                // Step #2 - prepare a plan to upload random Orders


                //Create 5000 orders
                for (int i = 1; i < 5000; i = i + 1)
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

                        //Pick random variant index from selected product
                        var variantNum = rnd.Next(0, products[productNum].Variants.Count-1);

                        //Set values to be inserted into order line item
                        var variantId = products[productNum].Variants[variantNum].Id;
                        var price = products[productNum].Variants[variantNum].Price;
                        var sku = products[productNum].Variants[variantNum].Sku;
                        var title = products[productNum].Variants[variantNum].Title;



                    }

                }


                // Step #3 - Insert Orders into Shopify API

                //This is a sample of what we need to generate:
                //
                //POST / admin / orders.json
                //{
                //  "order": {
                //             "line_items": [
                //                          {
                //                            "title": "Product title",
                //                            "price": 74.99,
                //                            "grams": "1300",
                //                            "quantity": 3,
                //                            "tax_lines": [
                //                              {
                //                                "price": 13.5,
                //                                "rate": 0.06,
                //                                "title": "State tax"
                //                              }
                //                                         ]
                //                          }
                //                            ],
                //    "transactions": [
                //      {
                //		  "test": "true",
                //        "kind": "sale",
                //        "status": "success",
                //        "amount": 238.47
                //      }
                //                     ],
                //    "total_tax": 13.5,
                //    "currency": "USD"
                //   }
                //}




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
