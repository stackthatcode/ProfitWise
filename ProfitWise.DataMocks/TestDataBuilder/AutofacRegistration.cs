using Autofac;

namespace ProfitWise.TestData.TestDataBuilder
{
    public class AutofacRegistration
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<OrderMakerProcess>();
        }
    }
}
