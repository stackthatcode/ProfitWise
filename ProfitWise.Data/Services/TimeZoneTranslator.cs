using System;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Services
{
    public class TimeZoneTranslator
    {
        // Returns Date + Midnight of that Date in another Time Zone based on *now* in UTC
        public DateTime Today(string shopifyTimeZone)
        {
            return FromUtcToShopifyTimeZone(DateTime.UtcNow, shopifyTimeZone).DateOnly();
        }

        public DateTime FromUtcToShopifyTimeZone(DateTime dateTimeUtc, string shopifyTimeZone)
        {
            var hourAdjustment = shopifyTimeZone.ParseShopifyTimeZoneHours();
            var minuteAdjustment = shopifyTimeZone.ParseShopifyTimeZoneMinutes();

            return dateTimeUtc.Add(new TimeSpan(0, hourAdjustment, minuteAdjustment, 0));
        }

        public DateTime ToUtcFromShopifyTimeZone(DateTime input, string shopifyTimeZone)
        {
            var hourAdjustment = -shopifyTimeZone.ParseShopifyTimeZoneHours();
            var minAdjustment = -shopifyTimeZone.ParseShopifyTimeZoneMinutes();

            return input.Add(new TimeSpan(0, hourAdjustment, minAdjustment, 0));
        }        
    }
}
