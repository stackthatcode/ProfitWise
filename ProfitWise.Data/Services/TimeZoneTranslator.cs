using System;
using ProfitWise.Data.Utility;

namespace ProfitWise.Data.Services
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


        public DateTime TranslateToTimeZone(DateTime input, string destinationTimeZone)
        {
            var destinationHourOffset = destinationTimeZone.ParseTimeZoneHours();
            var destinationMinuteOffset = destinationTimeZone.ParseTimeZoneMinutes();

            var hourAdjustment = destinationHourOffset - MachineHourOffset;
            var minAdjustment = destinationMinuteOffset - MachineMinuteOffset;

            return input.Add(new TimeSpan(0, hourAdjustment, minAdjustment, 0));
        }

        public DateTime TranslateFromTimeZone(DateTime input, string sourceTimeZone)
        {
            var sourceHourOffset = sourceTimeZone.ParseTimeZoneHours();
            var sourceMinuteOffset = sourceTimeZone.ParseTimeZoneMinutes();

            var hourAdjustment = -(sourceHourOffset - MachineHourOffset);
            var minAdjustment = -(sourceMinuteOffset - MachineMinuteOffset);

            return input.Add(new TimeSpan(0, hourAdjustment, minAdjustment, 0));
        }


        // 1:00 PM CST => Input / Server Time
        // 10:00 AM ALASKA => Shopify
    }
}
