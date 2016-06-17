using System;
using Castle.DynamicProxy;
using ProfitWise.Data.Repositories;

namespace ProfitWise.Data.Aspect
{
    public class UserIdRequired : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var userIdConsumer = invocation.InvocationTarget as IUserIdConsumer;
            if (userIdConsumer == null)
            {
                throw new Exception(
                    $"{invocation.InvocationTarget.GetType()} does not implement IUserIdConsumer." +
                    "Please implement interface or remove the interceptor attribute.");
            }
            if (userIdConsumer.UserId == null)
            {
                throw new Exception("{invocation.InvocationTarget.GetType()} requires a non-null UserId before invoking any methods.");
            }

            invocation.Proceed();
        }
    }
}
