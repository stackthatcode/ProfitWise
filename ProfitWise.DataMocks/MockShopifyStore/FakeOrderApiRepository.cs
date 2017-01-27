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
    public class FakeOrderApiRepository : IOrderApiRepository
    {
        public static readonly List<Order> Orders = new List<Order>();
        public const int NumberOfOrders = 10000;

        static FakeOrderApiRepository()
        {
            var orderId = 1;

            while (orderId <= NumberOfOrders)
            {
                var createdAt = DateTime.Now.AddDays(-HelperExtensions.GenerateRandomInteger(1000));

                var order = new Order()
                {
                    Id = orderId,
                    LineItems = new List<OrderLineItem>(),
                    CancelledAt = null,
                    Tags = null,
                    Name = HelperExtensions.GenerateRandomString(30),
                    Refunds = new List<Refund>(),
                    CreatedAt = createdAt,
                    TotalTax = HelperExtensions.GenerateRandomInteger(1000),
                    FinancialStatus = null,
                    Email = HelperExtensions.GenerateRandomString(20),
                    UpdatedAt = createdAt,
                    OrderDiscount = 0,
                    SubTotal = HelperExtensions.GenerateRandomInteger(1000),
                };

                var lineCounter = 0;
                while (lineCounter++ < 3)
                {
                    var product = FakeProductApiRepository.Products.GetRandomItem();
                    var variant = product.Variants.GetRandomItem();

                    var lineItem = new OrderLineItem()
                    {
                        ParentOrder = order,
                        Discount = 0m,
                        Id = lineCounter,
                        Sku = variant.Sku,
                        Name = null,
                        VariantTitle = variant.Title,
                        ProductTitle = variant.ParentProduct.Title,
                        Quantity = HelperExtensions.GenerateRandomInteger(10) + 1,
                        VariantId = variant.Id,
                        Price = variant.Price,
                        Vendor = variant.ParentProduct.Vendor,
                        ProductId = variant.ParentProduct.Id,
                    };

                    order.LineItems.Add(lineItem);
                }
                orderId++;

                Orders.Add(order);
            }
        }


        public ShopifyCredentials ShopifyCredentials { get; set; }
        public int RetrieveCount(OrderFilter filter)
        {
            return Orders.Count;
        }

        public IList<Order> Retrieve(OrderFilter filter, int page = 1, int limit = 250)
        {
            return Orders.Skip((page - 1)*limit).Take(limit).ToList();
        }
    }
}
