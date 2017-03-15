using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Helpers;

namespace Push.Shopify.Model
{
    public class Webhook
    {
        public long Id { get; set; }
        public string Topic { get; set; }
        public string Address { get; set; }
        public string Format { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }


        public const string UninstallTopic = "app/uninstalled";

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
                Created_At = webhook.created_at,
                Updated_At = webhook.updated_at,
                Format = webhook.format,
                Topic = webhook.topic,
            };
            return output;
        }
    }
}
