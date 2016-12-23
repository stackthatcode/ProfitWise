using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProfitWise.Data.Model.Reports;
using Push.Foundation.Utilities.Helpers;


namespace ProfitWise.Data.Model.Profit
{
    public class ReportSeries
    {
        [JsonIgnore]
        public ReportSeriesElement Parent { get; set; }

        public ReportGrouping GroupingType { get; set; }
        public string GroupingKey { get; set; }
        public string GroupingName { get; set; }
        public IList<ReportSeriesElement> Elements { get; set; }


        // "3D Printer:2016:Q4" 
        public string Identifier => Parent != null
            ? GroupingKey + ":" + Parent.CanonicalDateIdentifier()
            : GroupingKey;

        public string Name { get; set; }    // "3D Printers", "Ultimaker 2 Plus", "Taulman" etc.


        public void VisitElements(Action<ReportSeriesElement> action)
        {
            foreach (var element in Elements)
            {
                action(element);
                element.ChildSeries?.VisitElements(action);
            }
        }

        public void VisitSeries(Action<ReportSeries> action)
        {
            action(this);
            foreach (var element in Elements)
            {
                element.ChildSeries?.VisitSeries(action);
            }
        }
    }

    public class ReportSeriesElement
    {
        public PeriodType PeriodType { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int? Year { get; set; }
        public int? Quarter { get; set; }
        public int? Month { get; set; }
        public int? Week { get; set; }
        public int? Day { get; set; }

        [JsonIgnore]
        public ReportSeries Parent { get; set; }
        public ReportSeries ChildSeries { get; set; }

        public decimal Amount { get; set; }

        public string Drilldown => ChildSeries?.Identifier;

        public string DateLabel()
        {
            if (PeriodType.Year == PeriodType)
            {
                return Year.ToString();
            }
            if (PeriodType.Quarter == PeriodType)
            {
                return "Q" + Quarter + ", " + Year;
            }
            if (PeriodType.Month == PeriodType)
            {
                return Month.Value.ToShortMonthName() + " " + Year;
            }
            if (PeriodType.Week == PeriodType)
            {
                return "Week of " + Start.ToString("MM/dd/yyyy");
            }
            return Start.DayOfWeek + " " + Start.ToString("MM/dd/yyyy");
        }

        public string CanonicalDateIdentifier()
        {
            if (PeriodType.Year == PeriodType)
            {
                return Year.ToString();
            }
            if (PeriodType.Quarter == PeriodType)
            {
                return Year + ":Q" + Quarter;
            }
            if (PeriodType.Month == PeriodType)
            {
                return Year + ":M" + Month;
            }
            if (PeriodType.Week == PeriodType)
            {
                return Year + ":W" + Week;
            }
            // DataGranularity.Day
            return Year + ":" + Month + ":" + Day;
        }

        public override string ToString()
        {
            return $"Type:{PeriodType} Start:{Start} End:{End} Y:{Year} " + 
                $"Q:{Quarter} M:{Month} W:{Week} D:{Day} Amount:{Amount}";
        }

        public bool MatchByGroupingAndDate(DatePeriodTotal input)
        {
            return
                this.Parent.GroupingKey == input.GroupingKey &&
                this.Parent.GroupingName == input.GroupingName &&
                MatchByDate(input);
        }

        public bool MatchByDate(DatePeriodTotal input)
        {
            if (input.PeriodType != this.PeriodType)
            {
                return false;
            }
            if (this.PeriodType >= PeriodType.Year && input.Year != this.Year)
            {
                return false;
            }
            if (this.PeriodType >= PeriodType.Quarter && input.Quarter != this.Quarter)
            {
                return false;
            }
            if (this.PeriodType >= PeriodType.Month && input.Month != this.Month)
            {
                return false;
            }
            if (this.PeriodType >= PeriodType.Week && input.Week != this.Week)
            {
                return false;
            }
            if (this.PeriodType >= PeriodType.Day && input.Month != this.Day)
            {
                return false;
            }
            return true;
        }
    }
}

