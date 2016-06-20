using System.Configuration;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
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
            var autofacContainer = AutofacRegistration.Build();            

            ConfigureAuth(app, autofacContainer);
            SeedDefaultSecurityDataIfNeeded(autofacContainer);

            DependencyResolver.SetResolver(new AutofacDependencyResolver(autofacContainer));

            // Hangfire Configuration - TODO - move this into the ContainerBuilder
            var connectionString = ConfigurationManager.ConnectionStrings["HangFire"].ConnectionString;
            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);
            app.UseHangfireDashboard();
        }
    }
}
