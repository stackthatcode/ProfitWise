using System;
using System.Configuration;
using Hangfire;
using Push.Utilities.Logging;


namespace ProfitWise.Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap.ConfigureApp();
            DependencyInjectionSchool.Test();
            Console.ReadLine();
        }



        //private static void ProxyPoc()
        //{
        //    ProxyGenerator _generator = new ProxyGenerator();
        //    var logger = LoggerSingleton.Get();
        //    var freezableInterceptor = new ErrorForensicsInterceptor(logger);
        //    var proxy = _generator.CreateClassProxy<JaaamySimon>(freezableInterceptor );

        //    proxy.Method(1, "Bad Data");
        //}




        private static void InvokeRefreshServices()
        {
            var logger = LoggerSingleton.Get();
            try
            {
                // This is for simulation purposes - in the future, we'll load a list of Users from database
                var userId = "a4ae6621-57ec-4ca8-837f-0b439e3cc710";

                //ProxyPoc();
                var masterProcess = new MasterProcess(logger);
                masterProcess.Execute(userId);
            }
            catch (Exception e)
            {
                logger.Error(e);
                logger.Fatal(e.Message);


                var trace = new System.Diagnostics.StackTrace();
                
                //var frame = trace.GetFrame(1);
                //var methodName = frame.GetMethod().Name;
                //var properties = this.GetType().GetProperties();
                //var fields = this.GetType().GetFields(); // public fields
                //                                         // for example:
                //foreach (var prop in properties)
                //{
                //    var value = prop.GetValue(this, null);
                //}
                //foreach (var field in fields)
                //{
                //    var value = field.GetValue(this);
                //}
            }
        }



        // *** SAVE FOR NOW *** //
        private static void HangFireStuff()
        {
            LoggerSingleton.Get = NLoggerImpl.RegistrationFactory("Initializer");
            var logger = LoggerSingleton.Get();


            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);

                // Wait for graceful server shutdown.
                var server = new BackgroundJobServer();

                // Logger
                logger.Info("PROCESS START:" + DateTime.Now + Environment.NewLine);

                //RecurringJob.AddOrUpdate(() => LoggerSingleton.Get().Info("Update Job Starting"), "");
                //var id = BackgroundJob.Enqueue(() => Console.WriteLine("Hello, "));
                //BackgroundJob.ContinueWith(id, () => Console.WriteLine("world!"));

                //new Hangfire.BackgroundJobServer().

                //Console.ReadLine();
                //server.Dispose();

                //throw new Exception("Wassup???");
                // DoStuff();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Console.ReadLine();

            }
            finally
            {
                logger.Info(Environment.NewLine + "PROCESS FINISH:" + DateTime.Now + Environment.NewLine);
            }
        }
    }



}
