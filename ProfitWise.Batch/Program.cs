using System;
using Autofac;
using ProfitWise.Data.Processes;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap.ConfigureApp();
            using (var container = AutofacRegistration.Build())
            {
                RefreshServiceForSingleUser(container, args);

                Console.Write("Please hit enter...");
                Console.ReadLine();
            }
        }



        private static void RefreshServiceForSingleUser(IContainer container, string[] args)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var userId = "d56850fb-3fe7-4c66-a59d-20f755f5f1f4";
                //var userId = "57f0da58-6e74-41d5-90a9-736d09aa3b2f";


                //var currencyProcess = scope.Resolve<CurrencyProcess>();
                //currencyProcess.Execute();

                var refreshProcess = scope.Resolve<RefreshProcess>();
                refreshProcess.Execute(userId);

            }
        }


        private static void InvokeRefreshServices(IContainer container, string[] args)
        {
            var logger = container.Resolve<IPushLogger>();
            try
            {
                logger.Info("Hello! - Executing InvokeRefreshServices" + Environment.NewLine);
                SimulateRefreshServiceFor1000Users(container);
            }
            catch (Exception e)
            {
                logger.Error(e);

                // Add notification service call here
                logger.Fatal(e.Message);
            }
        }

        private static void SimulateRefreshServiceFor1000Users(IContainer container)
        {
            int artificialUserId = 1000;
            while (artificialUserId < 2000)
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var refreshProcess = scope.Resolve<RefreshProcess>();
                    refreshProcess.Execute(artificialUserId.ToString());
                }
                artificialUserId++;
            }
        }




        private static void TestTimeZoneTranslation(IContainer container)
        {
            var translator = container.Resolve<TimeZoneTranslator>();
            var result =
                translator.TranslateToTimeZone(
                    new DateTime(2016, 9, 9, 6, 30, 0),
                    "(GMT-06:00) Central Time (US & Canada)");
            var result2 =
                translator.TranslateToTimeZone(
                    new DateTime(2016, 9, 9, 6, 30, 0),
                    "(GMT-05:00) America/New_York");
        }

    }
}
