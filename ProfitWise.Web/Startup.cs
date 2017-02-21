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
            AuthConfig.Configure(app, autofacContainer);
            DefaultSecurityDataConfig.Execute(autofacContainer);

            HangFireStartup.Configure(app);
        }
    }
}
