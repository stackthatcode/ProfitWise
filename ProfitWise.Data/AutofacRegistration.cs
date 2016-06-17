using Autofac;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Repositories;

namespace ProfitWise.Data
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<ProductDataRepository>().EnableClassInterceptors();
            builder.RegisterType<VariantDataRepository>().EnableClassInterceptors();
            builder.RegisterType<SqlRepositoryFactory>().EnableClassInterceptors();
            builder.RegisterType<UserIdRequiredInterceptor>();
        }
    }
}
