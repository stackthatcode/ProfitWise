using System;
using System.Configuration;
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


        public static readonly string
                WebhookAddress = ConfigurationManager
                    .AppSettings
                    .GetAndTryParseAsString("UninstallWebHookAddress", "");

        public static Webhook MakeUninstallHookRequest()
        {
            var request = new Webhook()
            {
                Address = WebhookAddress,
                Format = "json",
                Topic = "app/uninstalled",
            };
            return request;
        }

        public static Webhook MakeAddressUpdateRequest(long id)
        {
            return new Webhook()
            {
                Id = id,
                Address = WebhookAddress,
            };
        }
    }

    public static class WebhookExtensions
    {
        public static Webhook ToWebhook(this string json)
        {
            dynamic parent = JsonConvert.DeserializeObject(json);
            var output = new Webhook
            {
                Id = parent.webhook.id,
                Address = parent.webhook.address,
                Created_At = parent.webhook.created_at,
                Updated_At = parent.webhook.updated_at,
                Format = parent.webhook.format,
                Topic = parent.webhook.topic,
            };
            return output;
        }
    }
}
