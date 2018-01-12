using System;
using Push.Foundation.Utilities.Helpers;
using TimeZoneConverter;

namespace ProfitWise.Data.Services
{
    public class TimeZoneTranslator
    {
        // Returns Date + Midnight of that Date in another Time Zone based on *now* in UTC
        public DateTime Today(string shopifyTimeZone)
        {
            return FromUtcToShopTz(DateTime.UtcNow, shopifyTimeZone).DateOnly();
        }

        public DateTime FromUtcToShopTz(DateTime dateTimeUtc, string shopifyTimeZone)
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
            return _translator.FromUtcToShopTz(dateTimeUtc, shopifyTimeZone);
        }

        public static DateTime ToUtcFromShopTz(this DateTime dateTimeUtc, string shopifyTimeZone)
        {
            return _translator.ToUtcFromShopifyTimeZone(dateTimeUtc, shopifyTimeZone);
        }

        public static DateTime FromUnspecifiedToTimeZone(
                    this DateTime unspecifiedDateTime, string targetTimeZone)
        {
            var dateTimeUtc = 
                _translator.ToUtcFromShopifyTimeZone(unspecifiedDateTime, targetTimeZone);
            return _translator.FromUtcToShopTz(dateTimeUtc, targetTimeZone);
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

