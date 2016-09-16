using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Model;

namespace ProfitWise.DataMocks
{
    [Intercept(typeof(ShopifyCredentialRequired))]
    public class FakeProductApiRepository : IProductApiRepository
    {
        public static readonly IList<string> Tags = new List<string>();
        public static readonly IList<string> Vendors = new List<string>();
        public static readonly IList<string> ProductTypes = new List<string>();
        public static readonly IList<Product> Products = new List<Product>();


        public const int MaxVariantsPerProduct = 20;
        public const int NumberOfProducts = 200;
        public const int NumberOfProductsTypes = 20;
        public const int NumberOfTags = 50;
        public const int NumberOfVendors = 10;


        private static readonly Random random = new Random();
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        
        static FakeProductApiRepository()
        {
            PopulateFakeTags();
            PopulateFakeProductTypes();
            PopulateFakeVendors();
            PopulateFakeProducts();
        }

        private static void PopulateFakeProductTypes()
        {
            var counter = 0;
            while (counter++ < NumberOfProductsTypes)
            {
                ProductTypes.Add(GenerateRandomString(10));
            }
        }
        private static void PopulateFakeTags()
        {
            var counter = 0;
            while (counter++ < NumberOfTags)
            {
                Tags.Add(GenerateRandomString(10));
            }
        }

        private static void PopulateFakeVendors()
        {
            var counter = 0;
            while (counter++ < NumberOfVendors)
            {
                Vendors.Add(GenerateRandomString(15));
            }
        }

        private static void PopulateFakeProducts()
        {
            var productId = 0;
            var variantId = 0;

            while (productId < NumberOfProducts)
            {
                productId++;

                var tags = new List<string>();
                var tagcount = 0;
                var tagForThisProduct = random.Next(5);
                while (tagcount++ < tagForThisProduct)
                {
                    tags.Add(Tags.GetRandomItem());
                }

                var product = new Product()
                {
                    Id = productId,
                    Tags = string.Join(",", tags),
                    ProductType = ProductTypes.GetRandomItem(),
                    Title = GenerateRandomString(50),
                    Vendor = Vendors.GetRandomItem(),
                    Variants = new List<Variant>()
                };

                var numberOfVariants = random.Next(MaxVariantsPerProduct + 1);
                var variantCounter = 0;
                while (variantCounter++ < numberOfVariants)
                {
                    variantId++;
                    var variant = new Variant()
                    {
                        Id = variantId,
                        Sku = GenerateRandomString(20),
                        Title = GenerateRandomString(20),
                        Inventory = random.Next(100),
                        ParentProduct = product,
                        Price = random.Next(100) * 20 - 0.01m,
                        UpdatedAt = DateTime.Now.AddDays(-random.Next(720)),
                        InventoryManagement = random.Next(10) > 3 ? "Shopfiy" : null,                        
                    };

                    product.Variants.Add(variant);
                }
                

                Products.Add(product);
            }
        }



        public ShopifyCredentials ShopifyCredentials { get; set; }

        public int RetrieveCount(ProductFilter filter)
        {
            return Products.Count;
        }

        public IList<Product> Retrieve(ProductFilter filter, int page = 1, int limit = 250)
        {
            return Products.Skip((page - 1)*limit).Take(limit).ToList();
        }
    }
}
