﻿using System;
using System.Collections.Generic;
using Autofac.Extras.DynamicProxy2;
using Castle.Core.Logging;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace Push.Shopify.Repositories
{
    [Intercept(typeof(ShopifyCredentialRequired))]
    public class OrderApiRepository : IShopifyCredentialConsumer
    {
        private readonly IShopifyHttpClient _client;
        private readonly ShopifyRequestFactory _requestFactory;
        private readonly IPushLogger _logger;

        public ShopifyCredentials ShopifyCredentials { get; set; }

        public OrderApiRepository(
                IShopifyHttpClient client, 
                ShopifyRequestFactory requestFactory,
                IPushLogger logger)
        {
            _client = client;
            _requestFactory = requestFactory;
            _logger = logger;
        }


        public virtual int RetrieveCount(OrderFilter filter)
        {
            var url = "/admin/orders/count.json?" + filter.ToQueryStringBuilder();
            var request = _requestFactory.HttpGet(ShopifyCredentials, url);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }

        public virtual IList<Order> Retrieve(OrderFilter filter, int page = 1, int limit = 250)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("page", page)
                    .Add("limit", limit)
                    .Add(filter.ToQueryStringBuilder())
                    .ToString();


            var path = string.Format("/admin/orders.json?" + querystring);
            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var results = new List<Order>();

            foreach (var order in parent.orders)
            {
                var orderIdCatcher = order.id;

                if (_logger.IsTraceEnabled)
                {
                    _logger.Trace($"Dump of Order ID: {order.id}" +
                                    Environment.NewLine +
                                    JsonConvert.SerializeObject(order));
                }

                _logger.Debug($"Translating Order {order.name} ({order.id})");

                var orderResult 
                    = new Order
                        {
                            Id = order.id,
                            Email = order.email,
                            Name = order.name,
                            SubTotal = order.subtotal_price,
                            TotalTax = order.total_tax,
                            CreatedAt = order.created_at,
                            UpdatedAt = order.updated_at,
                            LineItems = new List<OrderLineItem>(),
                            Refunds = new List<Refund>(),
                            OrderDiscount = 0.00m,
                            CancelledAt = order.cancelled_at,
                            FinancialStatus = order.financial_status,
                            Tags = order.tags,
                    };

                foreach (var discount_code in order.discount_codes)
                {
                    decimal amount = discount_code.amount;
                    orderResult.OrderDiscount += amount;
                }

                foreach (var line_item in order.line_items)
                {
                    _logger.Debug($"Translating Order Line Item {line_item.id}");

                    var orderLineItemResult = new OrderLineItem();

                    orderLineItemResult.Id = line_item.id;
                    orderLineItemResult.Discount = line_item.total_discount;
                    orderLineItemResult.ProductId = line_item.product_id;
                    orderLineItemResult.VariantId = line_item.variant_id;
                    orderLineItemResult.Price = line_item.price;
                    orderLineItemResult.Quantity = line_item.quantity;
                    orderLineItemResult.Sku = line_item.sku;

                    orderLineItemResult.ProductTitle = line_item.title;
                    orderLineItemResult.VariantTitle = line_item.variant_title;
                    orderLineItemResult.Name = line_item.name;
                    orderLineItemResult.ParentOrder = orderResult;

                    orderResult.LineItems.Add(orderLineItemResult);
                }

                foreach (var refund in order.refunds)
                {
                    _logger.Debug($"Translating Refund {refund.id}");

                    var refundResult = new Refund();
                    refundResult.ParentOrder = orderResult;
                    refundResult.Id = refund.id;                    
                    refundResult.LineItems = new List<RefundLineItem>();

                    foreach (var refundLineItems in refund.refund_line_items)
                    {
                        var resultRefundLineItem = new RefundLineItem();
                        resultRefundLineItem.Id = refundLineItems.id;
                        resultRefundLineItem.LineItemId = refundLineItems.line_item_id;
                        resultRefundLineItem.RestockQuantity = refundLineItems.quantity;    // TEST THIS!!!
                        resultRefundLineItem.ParentRefund = refundResult;
                        resultRefundLineItem.TaxRefund = 0m;

                        foreach (var taxline in refundLineItems.line_item.tax_lines)
                        {
                            decimal amount = taxline.price;
                            resultRefundLineItem.TaxRefund += amount;
                        }
                        refundResult.LineItems.Add(resultRefundLineItem);
                    }

                    refundResult.TransactionAmount = 0m;
                    foreach (var transaction in refund.transactions)
                    {
                        if (transaction.status == "success")
                        {
                            decimal amount = transaction.amount;
                            refundResult.TransactionAmount += amount;
                        }
                    }

                    refundResult.ShippingAdjustment = 0m;
                    foreach (var adjustment in refund.order_adjustments)
                    {
                        if (adjustment.kind == "shipping_refund")
                        {
                            decimal amount = adjustment.amount;
                            refundResult.ShippingAdjustment += amount;
                        }
                    }

                    orderResult.Refunds.Add(refundResult);
                }
                results.Add(orderResult); 
            }

            return results;
        }
    }
}
