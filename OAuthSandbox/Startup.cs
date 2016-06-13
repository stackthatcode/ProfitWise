using System.Configuration;
using Hangfire;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OAuthSandbox.Startup))]
namespace OAuthSandbox
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
