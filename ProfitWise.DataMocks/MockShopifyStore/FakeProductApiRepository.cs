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
                ProductTypes.Add(HelperExtensions.GenerateRandomString(10));
            }
        }

        private static void PopulateFakeTags()
        {
            var counter = 0;
            while (counter++ < NumberOfTags)
            {
                Tags.Add(HelperExtensions.GenerateRandomString(10));
            }
        }

        private static void PopulateFakeVendors()
        {
            var counter = 0;
            while (counter++ < NumberOfVendors)
            {
                Vendors.Add(HelperExtensions.GenerateRandomString(15));
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
                var tagForThisProduct = HelperExtensions.GenerateRandomInteger(5);
                while (tagcount++ < tagForThisProduct)
                {
                    tags.Add(Tags.GetRandomItem());
                }

                var product = new Product()
                {
                    Id = productId,
                    Tags = string.Join(",", tags),
                    ProductType = ProductTypes.GetRandomItem(),
                    Title = HelperExtensions.GenerateRandomString(50),
                    Vendor = Vendors.GetRandomItem(),
                    Variants = new List<Variant>()
                };

                var numberOfVariants = HelperExtensions.GenerateRandomInteger(MaxVariantsPerProduct) + 1;
                var variantCounter = 0;
                while (variantCounter++ < numberOfVariants)
                {
                    variantId++;
                    var variant = new Variant()
                    {
                        Id = variantId,
                        Sku = HelperExtensions.GenerateRandomString(20),
                        Title = HelperExtensions.GenerateRandomString(20),
                        Inventory = HelperExtensions.GenerateRandomInteger(100),
                        ParentProduct = product,
                        Price = HelperExtensions.GenerateRandomInteger(100) * 20 - 0.01m,
                        UpdatedAt = DateTime.UtcNow.AddDays(-HelperExtensions.GenerateRandomInteger(720)),
                        InventoryManagement = HelperExtensions.GenerateRandomInteger(10) > 3 ? "Shopfiy" : null,                        
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

        public IList<Product> Retrieve(ProductFilter filter, int page = 1, int limit = 250, bool includeCost = false)
        {
            return Products.Skip((page - 1) * limit).Take(limit).ToList();
        }
    }
}
