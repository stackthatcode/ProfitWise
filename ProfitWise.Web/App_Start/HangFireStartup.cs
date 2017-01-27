using Hangfire;
using Owin;

namespace ProfitWise.Web
{
    public class HangFireStartup
    {
        public static void Configure(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            // TODO - enable Dashboard in the Admin App
        }
    }
}
