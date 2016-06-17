using Autofac;
using Push.Foundation.Web.Identity;

namespace Push.Foundation.Web
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<ApplicationDbContext>();
            builder.RegisterType<ApplicationRoleManager>();
            builder.RegisterType<ApplicationRoleManager>();

        }
    }
}
