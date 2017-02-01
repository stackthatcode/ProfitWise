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


        public static List<DateRange> Factory()
        {
            return new List<DateRange>()
            {
                new DateRange(Today, "Today", DateTime.Today, DateTime.Today),
                new DateRange(Yesterday, "Yesterday", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1)),
                new DateRange(Last7Days, "Last 7 Days", DateTime.Today.AddDays(-7), DateTime.Today),
                new DateRange(Last30Days, "Last 30 Days", DateTime.Today.AddDays(-30), DateTime.Today),

                new DateRange(ThisMonth, "This Month", DateTime.Today.FirstOfMonth(), DateTime.Today.LastOfMonth()),

                new DateRange(LastMonth, "Last Month", DateTime.Today.AddMonths(-1).FirstOfMonth(),
                                DateTime.Today.AddMonths(-1).LastOfMonth()),

                new DateRange(ThisQuarter, "This Quarter", DateTime.Today.FirstOfQuarter(),
                                DateTime.Today.LastOfQuarter()),

                new DateRange(LastQuarter, "Last Quarter", DateTime.Today.AddMonths(-3).FirstOfQuarter(),
                                DateTime.Today.AddMonths(-3).LastOfQuarter()),

                new DateRange(ThisYear, "This Year", DateTime.Today.FirstOfYear(), DateTime.Today.LastOfYear()),

                new DateRange(LastMonth, "Last Year", DateTime.Today.AddYears(-1).FirstOfYear(), DateTime.Today.AddYears(-1).LastOfYear()),
            };
        }
    }
}

