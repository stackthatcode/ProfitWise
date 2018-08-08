using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Push.Shopify.Model
{
    public class Webhook
    {
        public long Id { get; set; }
        public string Topic { get; set; }
        public string Address { get; set; }
        public string Format { get; set; }
        public DateTime Created_At_ShopTz { get; set; }
        public DateTime Updated_At_ShopTz { get; set; }


        public const string UninstallTopic = "app/uninstalled";
        public const string CustomerRedactTopic = "customers/redact";
        public const string ShopRedactTopic = "shop/redact";
        public const string CustomerDataRequestTopic = "customers/data_request";


        public static Webhook MakeUninstallHookRequest(string address)
        {
            var request = new Webhook()
            {
                Address = address,
                Format = "json",
                Topic = UninstallTopic,
            };
            return request;
        }

        public static Webhook MakeAddressUpdateRequest(long id, string address)
        {
            return new Webhook()
            {
                Id = id,
                Address = address,
            };
        }
    }

    public static class WebhookExtensions
    {
        public static Webhook ToSingleWebhook(this string json)
        {
            dynamic parent = JsonConvert.DeserializeObject(json);
            return ToSingleWebhook(parent.webhook);
        }

        public static List<Webhook> ToMultipleWebhooks(this string json)
        {
            dynamic parent = JsonConvert.DeserializeObject(json);
            var output = new List<Webhook>();
            foreach (var webhook in parent.webhooks)
            {
                output.Add(ToSingleWebhook(webhook));
            }

            return output;
        }

        public static Webhook ToSingleWebhook(dynamic webhook)
        {
            var output = new Webhook
            {
                Id = webhook.id,
                Address = webhook.address,
                Created_At_ShopTz = webhook.created_at,
                Updated_At_ShopTz = webhook.updated_at,
                Format = webhook.format,
                Topic = webhook.topic,
            };
            return output;
        }
    }
}
