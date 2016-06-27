using System;
using Castle.DynamicProxy;

namespace Push.Shopify.Aspect
{
    public class ShopifyCredentialRequired : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var shopifyCredentialConsumer = invocation.InvocationTarget as IShopifyCredentialConsumer;
            if (shopifyCredentialConsumer == null)
            {
                throw new Exception(
                    $"{invocation.InvocationTarget.GetType()} does not implement IShopifyCredentialConsumer." +
                    "Please implement interface or remove the interceptor attribute." );
            }
            if (shopifyCredentialConsumer.ShopifyCredentials == null)
            {
                throw new Exception("{invocation.InvocationTarget.GetType()} requires a non-null ShopifyCredentials before invoking any methods.");
            }

            invocation.Proceed();
        }
    }
}
