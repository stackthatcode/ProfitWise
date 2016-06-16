using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Push.Shopify.Repositories;

namespace ProfitWise.Batch.Factory
{
    public class ApiRepositoryFactory
    {
        public ProductApiRepository MakeForProducts(IContainer container, string userId)
        {
            return container.
        }

        public OrderApiRepository MakeForOrders(IContainer container, string userId)
        {

        }

    }
}
