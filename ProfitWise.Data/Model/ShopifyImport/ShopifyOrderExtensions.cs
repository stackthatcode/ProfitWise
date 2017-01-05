using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.Shopify;
using Push.Shopify.Model;

namespace ProfitWise.Data.Model.ShopifyImport
{
    public static class ShopifyOrderExtensions
    {
        public static void AppendLineItems(
                    this IList<ShopifyOrder> orders, IList<ShopifyOrderLineItem> lineItems)
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

        public static ShopifyOrder ToShopifyOrder(this Order order, int pwShopId)
        {
            var shopifyOrder = new ShopifyOrder()
            {
                PwShopId = pwShopId,
                ShopifyOrderId = order.Id,
                Email = order.Email,
                OrderNumber = order.Name,

                OrderLevelDiscount = order.OrderDiscount,
                FinancialStatus = order.FinancialStatus,

                Tags = order.Tags,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                LineItems = new List<ShopifyOrderLineItem>(),
                Adjustments = new List<ShopifyOrderAdjustment>(),

                Cancelled = order.CancelledAt.HasValue, // only used during Refresh to DELETE Cancelled Orders
            };

            foreach (var lineItem in order.LineItems)
            {
                shopifyOrder.LineItems.Add(lineItem.ToShopifyOrderLineItem(pwShopId, shopifyOrder));
            }

            foreach (var adjustment in order.NonShippingAdjustments)
            {
                shopifyOrder.Adjustments.Add(adjustment.ToShopifyOrderAdjustment(pwShopId, shopifyOrder));
            }            

            return shopifyOrder;
        }

        public static ShopifyOrderLineItem 
                ToShopifyOrderLineItem(
                    this OrderLineItem lineFromShopify, int pwShopId, ShopifyOrder parentOrder)
        {
            var newLineItem = new ShopifyOrderLineItem();
            newLineItem.PwShopId = pwShopId;
            newLineItem.ParentOrder = parentOrder;
            newLineItem.OrderDate = parentOrder.CreatedAt.Date;
            newLineItem.OrderDateTimestamp = parentOrder.CreatedAt;
            newLineItem.ShopifyOrderId = parentOrder.ShopifyOrderId;
            newLineItem.ShopifyOrderLineId = lineFromShopify.Id;
            newLineItem.UnitPrice = lineFromShopify.Price;
            newLineItem.Quantity = lineFromShopify.Quantity;
            newLineItem.LineDiscount = lineFromShopify.Discount;
            newLineItem.Refunds = new List<ShopifyOrderLineRefund>();

            foreach (var refundLineFromShopify in lineFromShopify.RefundLineItems)
            {                
                newLineItem.Refunds.Add(
                    refundLineFromShopify.ToShopifyOrderLineRefund(pwShopId, parentOrder, newLineItem));
            }

            return newLineItem;
        }

        public static ShopifyOrderLineRefund ToShopifyOrderLineRefund(
                this RefundLineItem refundLineFromShopify, int pwShopId, ShopifyOrder parentOrder, ShopifyOrderLineItem parentLineItem)
        {
            var newRefund = new ShopifyOrderLineRefund();
            newRefund.PwShopId = pwShopId;
            newRefund.ShopifyRefundId = refundLineFromShopify.Id;
            newRefund.ShopifyOrderId = parentOrder.ShopifyOrderId;
            newRefund.ShopifyOrderLineId = refundLineFromShopify.LineItemId;
            newRefund.OrderLineItem = parentLineItem;
            newRefund.Amount = refundLineFromShopify.SubTotal;
            newRefund.RefundDate = refundLineFromShopify.ParentRefund.CreatedAt;
            return newRefund;
        }


        public static ShopifyOrderAdjustment ToShopifyOrderAdjustment(
                this OrderAdjustment shopifyAdjustment, int pwShopId, ShopifyOrder parentOrder)
        {
            var adjustment = new ShopifyOrderAdjustment();
            adjustment.PwShopId = pwShopId;
            adjustment.ShopifyAdjustmentId = shopifyAdjustment.Id;
            adjustment.ShopifyOrderId = parentOrder.ShopifyOrderId;
            adjustment.Order = parentOrder;
            adjustment.AdjustmentDate = shopifyAdjustment.Refund.CreatedAt;
            adjustment.Amount = shopifyAdjustment.Amount;
            adjustment.TaxAmount = shopifyAdjustment.TaxAmount;
            adjustment.Kind = shopifyAdjustment.Kind;
            adjustment.Reason = shopifyAdjustment.Reason;
            return adjustment;
        }

        public static void CopyIntoExistingOrderForUpdate(this ShopifyOrder importedOrder, ShopifyOrder existingOrder)
        {
            existingOrder.UpdatedAt = importedOrder.UpdatedAt;
            existingOrder.Email = importedOrder.Email;
            existingOrder.Tags = importedOrder.Tags;
            existingOrder.FinancialStatus = importedOrder.FinancialStatus;

            //existingOrder.TotalRefund = importedOrder.TotalRefund;
            //existingOrder.TaxRefundAmount = importedOrder.TaxRefundAmount;
            //existingOrder.ShippingRefundAmount = importedOrder.ShippingRefundAmount;
        }
    }
}
