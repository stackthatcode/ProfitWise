using System.Collections.Generic;
using System.Linq;
using Push.Shopify.Model;

namespace ProfitWise.Data.Model
{
    public static class ShopifyOrderExtensions
    {
        public static void LoadLineItems(this IList<ShopifyOrder> orders, IList<ShopifyOrderLineItem> lineItems)
        {
            foreach (var order in orders)
            {
                order.LineItems = lineItems.Where(x => x.ShopifyOrderId == order.ShopifyOrderId).ToList();
                foreach (var lineitem in order.LineItems)
                {
                    lineitem.ParentOrder = order;
                }
            }
        }


        public static ShopifyOrder ToShopifyOrder(this Order order, int shopId)
        {
            var shopifyOrder = new ShopifyOrder()
            {
                ShopId = shopId,
                Email = order.Email,
                OrderNumber = order.Name,
                ShopifyOrderId = order.Id,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                SubTotal = order.SubTotal,
                TotalRefund = order.TotalRefunds,
                TaxRefundAmount = order.TotalTaxRefunds,
                ShippingRefundAmount = order.TotalShippingRefund,
                OrderLevelDiscount = order.OrderDiscount,
                LineItems = new List<ShopifyOrderLineItem>(),
                Cancelled = order.CancelledAt.HasValue,
            };

            foreach (var line_item in order.LineItems)
            {
                var shopifyOrderLineItem = new ShopifyOrderLineItem();
                shopifyOrderLineItem.ShopId = shopId;
                shopifyOrderLineItem.ParentOrder = shopifyOrder;
                shopifyOrderLineItem.ShopifyVariantId = line_item.VariantId;
                shopifyOrderLineItem.ShopifyProductId = line_item.ProductId;
                shopifyOrderLineItem.ShopifyOrderId = order.Id;
                shopifyOrderLineItem.ShopifyOrderLineId = line_item.Id;
                shopifyOrderLineItem.Sku = line_item.Sku;

                shopifyOrderLineItem.ProductTitle = line_item.ProductTitle;
                shopifyOrderLineItem.VariantTitle = line_item.VariantTitle;
                shopifyOrderLineItem.Name = line_item.Name;

                shopifyOrderLineItem.UnitPrice = line_item.Price;
                shopifyOrderLineItem.Quantity = line_item.Quantity;
                shopifyOrderLineItem.TotalRestockedQuantity = line_item.TotalRestockQuantity;
                shopifyOrderLineItem.TotalDiscount = line_item.Discount;

                shopifyOrder.LineItems.Add(shopifyOrderLineItem);
            }

            return shopifyOrder;
        }
    }
}
