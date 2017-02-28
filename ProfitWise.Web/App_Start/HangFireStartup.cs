using Hangfire;
using Owin;
using ProfitWise.Web.Attributes;

namespace ProfitWise.Web
{
    public class HangFireStartup
    {
        public static void Configure(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard(
                "/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new HangFireAuthorizationFilter(),  }
                });
        }
    }
}
