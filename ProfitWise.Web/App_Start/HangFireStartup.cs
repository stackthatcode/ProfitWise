using System;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using ProfitWise.Web;

namespace ProfitWise.Web
{
    public class HangFireStartup
    {
        public static void Configure(IAppBuilder app)
        {
            var options = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(1) // Default value
            };
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection", options);
        }
    }
}
