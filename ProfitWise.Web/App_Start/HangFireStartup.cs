using Hangfire;
using Owin;

namespace ProfitWise.Web
{
    public class HangFireStartup
    {
        public static void Configure(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard("/hangfire");

            // TODO - add filter that only allows Admins to view
        }
    }
}
