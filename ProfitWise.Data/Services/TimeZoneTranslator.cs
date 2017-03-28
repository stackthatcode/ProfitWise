﻿using System;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Helpers;
using TimeZoneConverter;

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
            var timeZoneId = TZConvert.IanaToWindows(shopifyTimeZone);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, timeZoneInfo);
        }

        public DateTime ToUtcFromShopifyTimeZone(DateTime dateTimeLocal, string shopifyTimeZone)
        {
            var dateTimeLocalTz = DateTime.SpecifyKind(dateTimeLocal, DateTimeKind.Unspecified);
            var timeZoneId = TZConvert.IanaToWindows(shopifyTimeZone);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(dateTimeLocalTz, timeZoneInfo);
        }
    }


    // After all that dependency injecting hemming and hawwing, eh?
    public static class TimeZoneTranslatorExtensions
    {
        private static readonly TimeZoneTranslator _translator = new TimeZoneTranslator();

        public static DateTime Today(string shopifyTimeZone)
        {
            return _translator.Today(shopifyTimeZone);
        }

        public static DateTime ToShopTimeZone(this DateTime dateTimeUtc, string shopifyTimeZone)
        {
            return _translator.FromUtcToShopifyTimeZone(dateTimeUtc, shopifyTimeZone);
        }

        public static DateTime ToUtc(this DateTime dateTimeUtc, string shopifyTimeZone)
        {
            return _translator.ToUtcFromShopifyTimeZone(dateTimeUtc, shopifyTimeZone);
        }

        public static DateTime FromUnspecifiedToLocalTimeZone(
                    this DateTime unspecifiedDateTime, string targetTimeZone)
        {
            var dateTimeUtc = 
                _translator.ToUtcFromShopifyTimeZone(unspecifiedDateTime, targetTimeZone);
            return _translator.FromUtcToShopifyTimeZone(dateTimeUtc, targetTimeZone);
        }

        public static DateTime? AsUtc(this DateTime? unspecifiedDateTime)
        {
            return unspecifiedDateTime?.AsUtc();
        }

        public static DateTime AsUtc(this DateTime unspecifiedDateTime)
        {
            return DateTime.SpecifyKind(unspecifiedDateTime, DateTimeKind.Utc);
        }
    }
}

