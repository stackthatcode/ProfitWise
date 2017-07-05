using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Preferences
{
    public class DateRangeDefaults
    {
        public const int Today = 1;
        public const int Yesterday = 2;
        public const int Last7Days = 3;
        public const int Last30Days = 4;
        public const int ThisMonth = 5;
        public const int LastMonth = 6;
        public const int ThisQuarter = 7;
        public const int LastQuarter = 8;
        public const int ThisYear = 9;
        public const int LastYear = 10;


        public static List<DateRange> Factory(DateTime today)
        {
            return new List<DateRange>()
            {
                new DateRange(Today, "Today", today, today),
                new DateRange(Yesterday, "Yesterday", today.AddDays(-1), today.AddDays(-1)),

                new DateRange(Last7Days, "Last 7 Days", today.AddDays(-7), today.AddDays(-1)),
                new DateRange(Last30Days, "Last 30 Days", today.AddDays(-30), today.AddDays(-1)),

                new DateRange(ThisMonth, "This Month", today.FirstOfMonth(), today.LastOfMonth()),
                new DateRange(LastMonth, "Last Month", today.AddMonths(-1).FirstOfMonth(),
                                today.AddMonths(-1).LastOfMonth()),

                new DateRange(ThisQuarter, "This Quarter", today.FirstOfQuarter(),
                                today.LastOfQuarter()),
                new DateRange(LastQuarter, "Last Quarter", today.AddMonths(-3).FirstOfQuarter(),
                                today.AddMonths(-3).LastOfQuarter()),

                new DateRange(ThisYear, "This Year", today.FirstOfYear(), today.LastOfYear()),
                new DateRange(LastYear, "Last Year", today.AddYears(-1).FirstOfYear(), today.AddYears(-1).LastOfYear()),
            };
        }
    }
}

