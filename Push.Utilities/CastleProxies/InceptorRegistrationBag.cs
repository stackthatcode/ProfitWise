using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Extras.DynamicProxy2;
using Castle.DynamicProxy;

namespace Push.Utilities.CastleProxies
{
    public class InceptorRegistry
    {
        readonly  IList<Type> _registry = new List<Type>();

        public void Add(Type type)
        {
            _registry.Add(type);
        }

        public IList<Type> Flush()
        {
            return _registry;
        }

        public int Count()
        {
            return _registry.Count;
        }
    }

    public static class AutofacInterceptorExtensions
    {
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            EnableClassInterceptorsWithRegistry<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registrationBuilder, InceptorRegistry interceptorRegistry) 
                    where TActivatorData : ConcreteReflectionActivatorData
        {
            if (interceptorRegistry.Count() == 0)
                return registrationBuilder;

            registrationBuilder.EnableClassInterceptors();
            foreach (var interceptorType in interceptorRegistry.Flush())
            {
                registrationBuilder.InterceptedBy(interceptorType);
            }
            return registrationBuilder;            ;
        }

    }
}

