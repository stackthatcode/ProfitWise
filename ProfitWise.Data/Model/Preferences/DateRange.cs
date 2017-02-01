using System;

namespace ProfitWise.Data.Model.Preferences
{
    public class DateRange
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateRange(int id, string name, DateTime startDate, DateTime endDate)
        {
            Id = id;
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
