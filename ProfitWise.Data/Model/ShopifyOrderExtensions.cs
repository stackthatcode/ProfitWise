using System.Collections.Generic;
using Push.Shopify.Model;

namespace ProfitWise.Data.Model
{
    public static class ShopifyOrderExtensions
    {
        public static ShopifyOrder ToShopifyOrder(this Order order, int shopId)
        {
            var shopifyOrder = new ShopifyOrder()
            {
                ShopId = shopId,
                TotalPrice = order.TotalPrice,
                Email = order.Email,
                OrderNumber = order.Name,
                ShopifyOrderId = order.Id,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                LineItems = new List<ShopifyOrderLineItem>()
            };

            foreach (var line_item in order.LineItems)
            {
                var shopifyOrderLineItem = new ShopifyOrderLineItem();
                shopifyOrderLineItem.ShopId = shopId;
                shopifyOrderLineItem.ShopifyVariantId = line_item.VariantId;
                shopifyOrderLineItem.ShopifyProductId = line_item.ProductId;
                shopifyOrderLineItem.ShopifyOrderId = order.Id;
                shopifyOrderLineItem.ShopifyOrderLineId = line_item.Id;
                shopifyOrderLineItem.Quantity = line_item.Quantity;
                shopifyOrderLineItem.Sku = line_item.Sku;

                shopifyOrderLineItem.ProductTitle = line_item.ProductTitle;
                shopifyOrderLineItem.VariantTitle = line_item.VariantTitle;
                shopifyOrderLineItem.Name = line_item.Name;

                shopifyOrderLineItem.UnitPrice = line_item.Price;

                shopifyOrder.LineItems.Add(shopifyOrderLineItem);
            }

            return shopifyOrder;
        }
    }
}
