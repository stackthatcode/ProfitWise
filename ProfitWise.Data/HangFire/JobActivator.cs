using System;
using Autofac;
using Hangfire;

namespace ProfitWise.Data.HangFire
{
    public class ContainerJobActivator : JobActivator
    {
        private readonly IContainer _container;

        public ContainerJobActivator(IContainer container)
        {
            _container = container;
        }

        public override object ActivateJob(Type type)
        {
            return _container.Resolve(type);
        }
    }
}

