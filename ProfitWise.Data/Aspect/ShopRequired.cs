using System;
using Castle.DynamicProxy;

namespace ProfitWise.Data.Aspect
{
    public class ShopRequired : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var shopIdFilter = invocation.InvocationTarget as IShopFilter;
            if (shopIdFilter == null)
            {
                throw new Exception(
                    $"{invocation.InvocationTarget.GetType()} does not implement IShopIdFilter." +
                    "Please assign and implement that interface or remove the interceptor attribute.");
            }
            if (shopIdFilter.PwShop == null)
            {
                throw new Exception("{invocation.InvocationTarget.GetType()} requires a non-null ShopId before invoking any methods.");
            }

            invocation.Proceed();
        }
    }
}
