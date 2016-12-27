using System;
using System.Linq;
using NUnit.Framework;
using Push.Foundation.Utilities.Helpers;


namespace ProfitWise.Test
{
    [TestFixture]
    public class GenericTests
    {
        [Test]
        public void PlayWithStrings()
        {
            var week1 = DateTime.Parse("11/27/2016").WeekOfYearIso8601();
            var week2 = DateTime.Parse("11/27/2016").WeekOfYearIso8601();
            var week3 = DateTime.Parse("11/29/2016").WeekOfYearIso8601();
            var week4 = DateTime.Parse("11/30/2016").WeekOfYearIso8601();
            var week5 = DateTime.Parse("12/1/2016").WeekOfYearIso8601();
            var week6 = DateTime.Parse("12/2/2016").WeekOfYearIso8601();
            var week7 = DateTime.Parse("12/3/2016").WeekOfYearIso8601();
            Assert.AreEqual(week1, week2);
            Assert.AreEqual(week2, week3);
            Assert.AreEqual(week3, week4);
            Assert.AreEqual(week4, week5);
            Assert.AreEqual(week5, week6);
            Assert.AreEqual(week6, week7);
        }

    }
}

