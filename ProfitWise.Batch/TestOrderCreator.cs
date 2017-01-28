using Autofac;
using ProfitWise.TestData.TestDataBuilder;

namespace ProfitWise.Batch
{
    public class TestOrderCreator
    {
        public static void Execute()
        {
            var container = Bootstrapper.ConfigureApp();

            using (var scope = container.BeginLifetimeScope())
            {
                var userId = "b805025c-d8ff-4a9e-80e7-01e42ddcdf78";

                var orderMakerProcess = scope.Resolve<OrderMakerProcess>();
                orderMakerProcess.Execute(userId);
            }
        }
    }
}
