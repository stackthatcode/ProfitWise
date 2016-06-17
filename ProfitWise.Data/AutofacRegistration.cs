using Autofac;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Repositories;
using Push.Utilities.Errors;

namespace ProfitWise.Data
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<ErrorForensics>();

            builder.RegisterType<ProductDataRepository>()
                .EnableClassInterceptors()  
                .InterceptedBy(typeof(ErrorForensics));

            builder.RegisterType<VariantDataRepository>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder.RegisterType<SqlRepositoryFactory>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(ErrorForensics));

            builder
                .RegisterType<UserIdRequired>();
        }
    }
}
