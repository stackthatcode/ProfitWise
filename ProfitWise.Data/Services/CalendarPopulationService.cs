using System;
using System.Globalization;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Repositories.System;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.Services
{
    public class CalendarPopulationService
    {
        private readonly SystemRepository _systemRepository;
        private readonly IPushLogger _logger;

        public CalendarPopulationService(
                SystemRepository systemRepository, IPushLogger logger)
        {
            _systemRepository = systemRepository;
            _logger = logger;
        }

        public void Execute()
        {
            var date = new DateTime(2006, 1, 1);
            var endDate = new DateTime(2067, 12, 31);
            while (date <= endDate)
            {
                var element = ReportSeriesFactory.GenerateElement(PeriodType.Day, date, date);

                //_logger.Info($"{element.Year} {element.Quarter} {element.Month} {element.Week} {element.Day} {dayOfWeek} {monthName} {dayName}");
                element.Week =
                    date.StartOfWeek(DayOfWeek.Sunday).Year * 100 +
                    date.StartOfWeek(DayOfWeek.Sunday).WeekOfYearIso8601();

                _systemRepository
                    .InsertCalendarEntry(
                        date, 
                        element.Year.Value, 
                        element.Quarter.Value, 
                        element.Month.Value,
                        element.Day.Value,
                        element.DayOfWeek.Value,
                        element.MonthName,
                        element.DayName,
                        element.Week.Value,
                        element.IsWeekday);

                date = date.AddDays(1);
            }
        }

    }
}
