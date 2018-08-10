using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Helpers;

namespace Push.Shopify.Model
{
    public class Topics
    {
        public const int UninstallTopicId = 1;
        public const int CustomerRedactTopicId = 2;
        public const int ShopRedactTopicId = 3;
        public const int CustomerDataRequestTopicId = 4;
        
        public static readonly
            Dictionary<int, string> Lookup
                = new Dictionary<int, string>
                    {
                        { 1, "app/uninstalled"},
                        { 2, "customers/redact" },
                        { 3, "shop/redact"},
                        { 4, "customers/data_request" }
                    };
    }


    public class RequiredWebhooks
    {
        private static readonly string
                UninstallWebhookAddr = ConfigurationManager
                    .AppSettings.GetAndTryParseAsString("UninstallWebHookAddress", "");

        private static readonly string
                CustomerRedactWebhookAddr = ConfigurationManager
                    .AppSettings.GetAndTryParseAsString("CustomerRedactWebHookAddress", "");

        private static readonly string
                ShopRedactWebhookAddr = ConfigurationManager
                    .AppSettings.GetAndTryParseAsString("ShopRedactWebHookAddress", "");

        private static readonly string
                CustomerDataRequestWebhookAddr = ConfigurationManager
                    .AppSettings.GetAndTryParseAsString("CustomerDataWebHookAddress", "");

        public static readonly List<Webhook> All = new List<Webhook>()
            {
                Webhook.MakeHookRequest(Topics.UninstallTopicId, UninstallWebhookAddr),
                Webhook.MakeHookRequest(Topics.CustomerRedactTopicId, CustomerRedactWebhookAddr),
                Webhook.MakeHookRequest(Topics.ShopRedactTopicId, ShopRedactWebhookAddr),
                Webhook.MakeHookRequest(Topics.CustomerDataRequestTopicId, CustomerDataRequestWebhookAddr),
            };

        public static readonly List<Webhook> Subscriptions = new List<Webhook>()
            {
                Webhook.MakeHookRequest(Topics.UninstallTopicId, UninstallWebhookAddr),
            };
    }

    public class Webhook
    {
        // For internally cataloguing
        [JsonIgnore]
        public int TopicId { get; set; }
        
        public long Id { get; set; }
        public string Topic { get; set; }
        public string Address { get; set; }
        public string Format { get; set; }
        public DateTime Created_At_ShopTz { get; set; }
        public DateTime Updated_At_ShopTz { get; set; }
        

        public static Webhook MakeHookRequest(int topicId, string address)
        {
            var request = new Webhook()
            {
                TopicId = topicId,
                Address = address,
                Format = "json",
                Topic = Topics.Lookup[topicId],
            };

            return request;
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
