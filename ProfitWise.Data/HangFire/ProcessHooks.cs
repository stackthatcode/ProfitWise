using System;

namespace ProfitWise.Data.HangFire
{
    public class ProcessHooks
    {
        // TODO - add registration of AutoFac ...

        public static void HelloWorld()
        {
            Console.WriteLine("Testing this HangFire stuff...");
        }

        public static void ErrorWorld()
        {
            throw new Exception("Boom! Testing HangFire's logging capabilities...");
        }
    }
}
