using System.Web.Optimization;

namespace ProfitWise.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Bundles/Utility")
                .Include("~/Scripts/Utility/jquery.min.js")
                .Include("~/Scripts/Utility/aQuery-1.0-min.js")
                .Include("~/Scripts/Utility/flow.js")
                .Include("~/Scripts/Utility/numeral.min.js")
                //.Include("~/Scripts/Utility/modernizr-2.6.2.js")
                .Include("~/Scripts/Utility/moment.js"));

            bundles.Add(new ScriptBundle("~/Bundles/Bootstrap")
                .Include("~/Scripts/Bootstrap/bootstrap.min.js")
                .Include("~/Scripts/Bootstrap/bootstrap-datetimepicker.min.js")
                .Include("~/Scripts/Bootstrap/daterangepicker.js"));

            bundles.Add(new ScriptBundle("~/Bundles/Highcharts")
                 .Include("~/Scripts/Highcharts/js/highcharts.js")
                 .Include("~/Scripts/Highcharts/js/modules/drilldown.js"));

            bundles.Add(new ScriptBundle("~/Bundles/KO")
                 .Include("~/Scripts/KO/knockout-3.3.0.js")
                 .Include("~/Scripts/KO/knockstrap.min.js"));

            bundles.Add(new ScriptBundle("~/Bundles/ProfitWise")
                .Include("~/Scripts/ProfitWise/ProfitWiseFunctions.js")
                .Include("~/Scripts/ProfitWise/ProfitWiseCurrency.js"));

            bundles.Add(new StyleBundle("~/Content/Bootstrap")
                .Include("~/Content/bootstrap-3.3.7-dist/css/bootstrap.min.css"));

            bundles.Add(new StyleBundle("~/Content/Css")
                .Include("~/Content/daterangepicker.css")
                .Include("~/Content/datetimepicker.css"));

            // BundleTable.EnableOptimizations = true;
        }
    }
}
