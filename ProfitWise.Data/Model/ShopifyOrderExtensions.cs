﻿using System.Collections.Generic;
using System.Linq;
using Push.Shopify.Model;

namespace ProfitWise.Data.Model
{
    public static class ShopifyOrderExtensions
    {
        public static void AppendLineItems(this IList<ShopifyOrder> orders, IList<ShopifyOrderLineItem> lineItems)
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
                PwShopId = shopId,
                ShopifyOrderId = order.Id,
                Email = order.Email,
                OrderNumber = order.Name,
                OrderLevelDiscount = order.OrderDiscount,
                SubTotal = order.SubTotal,
                TotalRefund = order.TotalRefunds,
                TaxRefundAmount = order.TotalTaxRefunds,
                ShippingRefundAmount = order.TotalShippingRefund,
                FinancialStatus = order.FinancialStatus,
                Tags = order.Tags,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                LineItems = new List<ShopifyOrderLineItem>(),
                Cancelled = order.CancelledAt.HasValue, // only used during Refresh to DELETE Cancelled Orders
            };
            return shopifyOrder;
        }

        public static ShopifyOrderLineItem ToShopifyOrderLineItem(
                this OrderLineItem line_item, long parentShopifyOrderId, int shopId)
        {
            var shopifyOrderLineItem = new ShopifyOrderLineItem();
            shopifyOrderLineItem.ShopId = shopId;

            shopifyOrderLineItem.OrderDate = line_item.ParentOrder.CreatedAt;
            shopifyOrderLineItem.ShopifyOrderId = parentShopifyOrderId;
            shopifyOrderLineItem.ShopifyOrderLineId = line_item.Id;

            shopifyOrderLineItem.UnitPrice = line_item.Price;
            shopifyOrderLineItem.Quantity = line_item.Quantity;
            shopifyOrderLineItem.TotalRestockedQuantity = line_item.TotalRestockQuantity;
            shopifyOrderLineItem.TotalDiscount = line_item.Discount;

            return shopifyOrderLineItem;
        }

        public static void CopyIntoExistingOrderForUpdate(this ShopifyOrder importedOrder, ShopifyOrder existingOrder)
        {
            existingOrder.UpdatedAt = importedOrder.UpdatedAt;
            existingOrder.Email = importedOrder.Email;
            existingOrder.TotalRefund = importedOrder.TotalRefund;
            existingOrder.TaxRefundAmount = importedOrder.TaxRefundAmount;
            existingOrder.ShippingRefundAmount = importedOrder.ShippingRefundAmount;
            existingOrder.Tags = importedOrder.Tags;
            existingOrder.FinancialStatus = importedOrder.FinancialStatus;
        }
    }
}
