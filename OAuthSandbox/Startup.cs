using System.Configuration;
using Hangfire;
using Microsoft.Owin;
using Owin;
using ProfitWise.Web;

[assembly: OwinStartupAttribute(typeof(Startup))]
namespace ProfitWise.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureDefaultSecurityData();

            var connectionString = ConfigurationManager.ConnectionStrings["HangFire"].ConnectionString;
            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);
            app.UseHangfireDashboard();
        }
    }
}
