﻿using System;
using ProfitWise.Data.Utility;

namespace ProfitWise.Data.Services
{
    /// <summary>
    /// This was motivated by the fact that Shopify's TimeZones ignore daylight savings e.g.
    /// Chicago / CST was always GMT-0:600
    /// </summary>
    public class TimeZoneTranslator
    {
        public int ServerHourOffset { get; private set; }
        public int ServerMinuteOffset { get; private set; }


        public TimeZoneTranslator(string machineTimeZone)
        {
            ServerHourOffset = machineTimeZone.ParseTimeZoneHours();
            ServerMinuteOffset = machineTimeZone.ParseTimeZoneMinutes();
        }

        public TimeZoneTranslator(int hour, int minute)
        {
            ServerHourOffset = hour;
            ServerMinuteOffset = minute;
        }

        // Returns Date + Midnight of that Date in another Time Zone based on *now*
        public DateTime Today(string otherTimeZone)
        {
            var serverTime = DateTime.Now;
            var timeInOtherTimeZone = ToOtherTimeZone(serverTime, otherTimeZone);

            // Strip off time
            var dateInOtherTimeZone = new DateTime(
                timeInOtherTimeZone.Year, timeInOtherTimeZone.Month, timeInOtherTimeZone.Day);

            return dateInOtherTimeZone;
        }

        public TimeSpan OtherTimeZoneAdjustment(string otherTimeZone)
        {
            var otherHourOffset = otherTimeZone.ParseTimeZoneHours();
            var otherMinuteOffset = otherTimeZone.ParseTimeZoneMinutes();

            var hourAdjustment = otherHourOffset - ServerHourOffset;
            var minuteAdjustment = otherMinuteOffset - ServerMinuteOffset;

            return new TimeSpan(0, hourAdjustment, minuteAdjustment, 0);
        }

        public DateTime ToOtherTimeZone(DateTime timeInServerTimeZone, string otherTimeZone)
        {
            var adjustment = OtherTimeZoneAdjustment(otherTimeZone);
            return timeInServerTimeZone.Add(adjustment);
        }

        // Haven't needed this, because all the Orders from Shopify contain Time Zone information,
        // ... which, when deserialized is parsed into Server Time automatically
        public DateTime ToServerTime(DateTime input, string otherTimeZone)
        {
            var otherHourOffset = otherTimeZone.ParseTimeZoneHours();
            var sourceMinuteOffset = otherTimeZone.ParseTimeZoneMinutes();

            var hourAdjustment = -(otherHourOffset - ServerHourOffset);
            var minAdjustment = -(sourceMinuteOffset - ServerMinuteOffset);

            return input.Add(new TimeSpan(0, hourAdjustment, minAdjustment, 0));
        }
        
        // 1:00 PM CST => Input / Server Time
        // 10:00 AM ALASKA => Shopify
    }
}
