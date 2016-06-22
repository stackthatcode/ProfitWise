using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                LineItems = new List<ShopifyOrderLineItem>()
            };

            foreach (var line_item in order.LineItems)
            {
                var shopifyOrderLineItem = new ShopifyOrderLineItem()
                {
                    ShopId = shopId,
                    VariantId = line_item.VariantId,
                    ProductId = line_item.ProductId,
                    ShopifyOrderId = order.Id,
                    ShopifyOrderLineId = line_item.Id,
                    Quantity = line_item.Quantity,
                    ReportedSku = line_item.Sku,
                    //TotalDiscount = line_item => ON HOLD
                    UnitPrice = line_item.Price,
                };
            }

            return shopifyOrder;
        }
    }
}
