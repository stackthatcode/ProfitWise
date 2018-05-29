using System.Web.Optimization;

namespace ProfitWise.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // File Upload jazz...
            bundles.Add(new ScriptBundle("~/Bundles/FileUpload")
                .Include("~/Scripts/FileUpload/js/vendor/jquery.ui.widget.js")
                .Include("~/Scripts/FileUpload/js/jquery.iframe-transport.js")
                .Include("~/Scripts/FileUpload/js/jquery.fileupload.js"));

            bundles.Add(new StyleBundle("~/StyleBundles/FileUpload")
                .Include("~/Scripts/FileUpload/css/jquery.fileupload.css"));


            bundles.Add(new ScriptBundle("~/Bundles/Utility")
                .Include("~/Scripts/Utility/jquery.min.js")
                .Include("~/Scripts/Utility/aQuery-1.0-min.js")
                .Include("~/Scripts/Utility/flow.js")
                .Include("~/Scripts/Utility/numeral.min.js")
                .Include("~/Scripts/Utility/bootstrap-tour.min.js")
                //.Include("~/Scripts/Utility/modernizr-2.6.2.js")
                .Include("~/Scripts/Utility/moment.js")
                .Include("~/Content/tether-1.3.3-dist/js/tether.min.js")
                .Include("~/Content/shepherd-dist/js/shepherd.min.js"));
            

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

            bundles.Add(new ScriptBundle("~/Bundles/ProfitWiseFunctions")
                .Include("~/Scripts/ProfitWise/ProfitWiseCurrency.js")
                .Include("~/Scripts/ProfitWise/ProfitWiseFunctions.js")
                .Include("~/Scripts/ProfitWise/ProfitWiseShopify.js"));

            bundles.Add(new ScriptBundle("~/Bundles/ProfitWiseFunctionsImpersonated")
                .Include("~/Scripts/ProfitWise/ProfitWiseCurrency.js")
                .Include("~/Scripts/ProfitWise/ProfitWiseFunctions.js")
                .Include("~/Scripts/ProfitWise/ProfitWiseShopifyImpersonated.js"));

            bundles.Add(new StyleBundle("~/Content/Bootstrap")
                .Include("~/Content/bootstrap-3.3.7-dist/css/bootstrap.min.css"));

            bundles.Add(new StyleBundle("~/Content/Css")
                .Include("~/Content/daterangepicker.css")
                .Include("~/Content/datetimepicker.css")
                .Include("~/Content/bootstrap-tour.css"));

            // BundleTable.EnableOptimizations = true;
        }
    }
}
