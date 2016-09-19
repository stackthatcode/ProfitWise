using System;

namespace Push.Shopify.TimeZone
{
    /// <summary>
    /// This was motivated by the fact that Shopify's TimeZones ignore daylight savings e.g.
    /// Chicago / CST was always GMT-0:600
    /// </summary>
    public class TimeZoneTranslator
    {
        public int MachineHourOffset { get; private set; }
        public int MachineMinuteOffset { get; private set; }

        public TimeZoneTranslator(string machineTimeZone)
        {
            MachineHourOffset = machineTimeZone.ParseTimeZoneHours();
            MachineMinuteOffset = machineTimeZone.ParseTimeZoneMinutes();
        }

        public TimeZoneTranslator(int hour, int minute)
        {
            MachineHourOffset = hour;
            MachineMinuteOffset = minute;
        }


        // *** This is used by Refresh Process
        public DateTime TranslateToShopifyTimeZone(DateTime input, string shopifyTimeZone)
        {
            var targetHourOffset = shopifyTimeZone.ParseTimeZoneHours();
            var targetMinuteOffset = shopifyTimeZone.ParseTimeZoneMinutes();

            var hourAdjustment = targetHourOffset - MachineHourOffset;
            var minAdjustment = targetMinuteOffset - MachineMinuteOffset;

            return input.Add(new TimeSpan(0, hourAdjustment, minAdjustment, 0));

            // 1:00 PM CST => Input / Server Time
            // 2:00 PM EST => Shopify
        }

    }
}
