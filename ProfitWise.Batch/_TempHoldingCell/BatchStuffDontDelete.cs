using System;
using Autofac;
using ProfitWise.Data.Services;

namespace ProfitWise.Batch._TempHoldingCell
{
    public class BatchStuffDontDelete
    {        
        public static void TestTimeZoneTranslation(IContainer container)
        {
            var translator = container.Resolve<TimeZoneTranslator>();

            var result =
                translator.ToOtherTimeZone(
                    new DateTime(2016, 9, 9, 6, 30, 0), "(GMT-06:00) Central Time (US & Canada)");

            var result2 =
                translator.ToOtherTimeZone(
                    new DateTime(2016, 9, 9, 6, 30, 0), "(GMT-05:00) America/New_York");
        }        
    }
}

