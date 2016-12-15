using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Push.Foundation.Utilities.General;


namespace ProfitWise.Test
{
    [TestFixture]
    public class GenericTests
    {
        [Test]
        public void PlayWithStrings()
        {
            var input = "Abc this   is    my test";
            var terms =
                input.Split(' ')
                    .Select(x => x.Trim())
                    .Where(x => x != "")
                    .ToList();
        }


        //[Test]
        //public void GenerateYearSeries()
        //{
        //    var output = ReportSeriesFactory.GenerateSeries(
        //        "3D Printers", DateTime.Parse("2016-03-15"), DateTime.Parse("2016-12-20"));
        //    output.Data.ForEach(x => Console.WriteLine(x));
        //}

        //[Test]
        //public void GenerateWeekSeries()
        //{
        //    var output = ReportSeriesFactory.GenerateSeries(
        //        "3D Printers", DateTime.Parse("2016-03-15"), DateTime.Parse("2016-5-5"));
        //    output.Data.ForEach(x => Console.WriteLine(x));
        //}

        //[Test]
        //public void GenerateMonthSeries()
        //{
        //    var output = ReportSeriesFactory.GenerateSeries(
        //        "3D Printers", DateTime.Parse("2016-03-15"), DateTime.Parse("2016-12-20"));
        //    output.Data.ForEach(x => Console.WriteLine(x));
        //}
    }
}

