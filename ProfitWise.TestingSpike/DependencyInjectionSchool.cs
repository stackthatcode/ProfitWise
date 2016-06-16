using Autofac;
using Autofac.Extras.DynamicProxy2;
using Push.Utilities.Logging;

namespace ProfitWise.Batch
{
    public class DependencyInjectionSchool
    {
        public static void Test2()
        {
            // Creating objects to do stuff
            var instanceOfX = new X();
            var instanceOfY = new Y(instanceOfX);
            var instanceOfZ1 = new Z1(instanceOfX, instanceOfY);
            var instanceOfZ2 = new Z2(instanceOfX, instanceOfY, instanceOfZ1);
        }

        public static void Test()
        {
            // Creating objects to do stuff
            var instanceOfX = new X();
            var instanceOfY = new Y(instanceOfX);
            var instanceOfZ1 = new Z1(instanceOfX, instanceOfY);
            var instanceOfZ2 = new Z2(instanceOfX, instanceOfY, instanceOfZ1);


            // Container Build
            var builder = new ContainerBuilder();
            builder.RegisterType<X>().EnableClassInterceptors();
            builder.RegisterType<Y>().EnableClassInterceptors();
            builder.RegisterType<Z1>().EnableClassInterceptors();
            builder.RegisterType<Z2>().EnableClassInterceptors();
            builder.Register(c => new NLoggerImpl("Test.Logger", x => x)).As<ILogger>();
            builder.Register(c => new LoggingInterceptor(c.Resolve<ILogger>()));

            var container = builder.Build();
            var injectedInstanceOfZ = container.Resolve<Z2>();
            injectedInstanceOfZ.Method();

        }


        [Intercept(typeof(LoggingInterceptor))]
        public class X
        {
            public virtual void Method()
            {                
            }
        }

        [Intercept(typeof(LoggingInterceptor))]
        public class Y
        {
            private readonly X _x;

            public Y(X x)
            {
                _x = x;
            }

            public virtual void Method()
            {
            }
        }

        [Intercept(typeof(LoggingInterceptor))]

        public class Z1
        {
            private readonly X _x;
            private readonly Y _y;

            public Z1(X x, Y y)
            {
                _x = x;
                _y = y;
            }

            public virtual void Method()
            {
            }
        }


        [Intercept(typeof(LoggingInterceptor))]
        public class Z2
        {
            private readonly X _x;
            private readonly Y _y;
            private readonly Z1 _z1;

            public Z2(X x, Y y, Z1 z1)
            {
                _x = x;
                _y = y;
                _z1 = z1;
            }
            public virtual void Method()
            {
            }
        }
    }
}
