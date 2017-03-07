using System;
using System.Linq;
using Autofac;
using Hangfire;
using Microsoft.AspNet.Identity.EntityFramework;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Processes;
using Push.Foundation.Web.Identity;


namespace ProfitWise.Batch
{
    class Program
    {
        private const string HangFireBackgroundServiceOption = "1";
        private const string ScheduleInitialShopRefreshOption = "2";
        private const string ScheduleBackgroundSystemJobsOption = "3";
        private const string AppUninstallTestRequest = "DEBUG";
        private const string ResetAdminPasswordOption = "W";


        private static void TestHmacVerification()
        {
            var input = System.IO.File.ReadAllText("Request.txt");
            Console.ReadLine();
        }



        static void Main(string[] args)
        {
            // Test HMAC encoder
            Console.WriteLine("ProfitWise.Batch v1.0 - Push Automated Commerce LLC");
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("");

            var choice =
                args.Length > 0 && args[0].ToLower() == "service"
                    ? HangFireBackgroundServiceOption
                    : SolicitUserInput();

            if (choice.Trim() == HangFireBackgroundServiceOption)
            {
                HangFireBackgroundService();
                return;
            }
            if (choice.Trim() == ScheduleBackgroundSystemJobsOption)
            {
                ScheduleBackgroundSystemJobs();
                return;
            }
            if (choice.Trim() == ScheduleInitialShopRefreshOption)
            {
                ScheduleInitialShopRefresh();
                return;
            }
            if (choice.Trim() == ResetAdminPasswordOption)
            {
                ResetAdminPassword();
                return;
            }
            if (choice.Trim() == AppUninstallTestRequest)
            {
                AppUninstallTest.Execute();
                return;
            }

            Console.WriteLine("Invalid option - exiting. Bye!");
        }

        public static void ExitWithAnyKey()
        {
            Console.WriteLine("Hit any key to exit... FIN");
            Console.ReadKey();
        }

        public static string SolicitUserInput()
        {
            Console.WriteLine("Please select from the following options and hit enter");
            Console.WriteLine($"{HangFireBackgroundServiceOption} - Run HangFire Background Service");
            Console.WriteLine($"{ScheduleInitialShopRefreshOption} - Schedule an Initial Shop Refresh");
            Console.WriteLine($"{ScheduleBackgroundSystemJobsOption} - Schedule the default ProfitWise System Jobs");
            Console.WriteLine("");
            return Console.ReadLine();
        }

        public static void ScheduleBackgroundSystemJobs()
        {
            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<HangFireService>();
                service.ScheduleExchangeRateRefresh();
                service.ScheduleSystemCleanupProcess();

                ExitWithAnyKey();
            }
        }

        public static void ScheduleInitialShopRefresh()
        {
            Console.WriteLine("");
            Console.WriteLine("Please enter the User Id:");
            var userId = Console.ReadLine();
            userId = userId == "" ? "ff692d3d-26ef-4a0f-aa90-0e24b4cfe26f" : userId;
            Console.WriteLine($"Using User Id: {userId}");

            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<HangFireService>();
                service.AddOrUpdateInitialShopRefresh(userId);
                
            }

            ExitWithAnyKey(); 
        }
        

        public static void HangFireBackgroundService()
        {
            Bootstrapper.ConfigureApp(true);
            var backgroundServerOptions 
                = new BackgroundJobServerOptions()
                    {
                        SchedulePollingInterval = new TimeSpan(0, 0, 0, 1),   
                        Queues = QueueChannel.All,
                    };

            using (var server = new BackgroundJobServer(backgroundServerOptions))
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }
        
        public static void OrderRefreshProcess(string userId)
        {
            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {   
                var refreshProcess = scope.Resolve<ShopRefreshProcess>();
                refreshProcess.RoutineShopRefresh(userId);

                Console.ReadLine();
            }
        }

        public static void ResetAdminPassword()
        {
            var newPassword = SolicitPassword("Enter your password: ");
            if (newPassword.Trim() == "")
            {
                return;
            }
            var confirmPassword = SolicitPassword("Re-enter your password: ");
            if (confirmPassword.Trim() == "")
            {
                return;
            }
            if (newPassword != confirmPassword)
            {
                Console.WriteLine("Passwords don't match - hit any key to exit");
                Console.Read();
                return;
            }

            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var context = scope.Resolve<ApplicationDbContext>();
                var manager = scope.Resolve<ApplicationUserManager>();
                var store = scope.Resolve<UserStore<ApplicationUser>>();

                var role = context.Roles.First(x => x.Name == "ADMIN");
                var adminUser = 
                    context.Users
                        .First(u => u.Roles.Select(r => r.RoleId).Contains(role.Id));

                var hashedNewPassword = manager.PasswordHasher.HashPassword(newPassword);
                ApplicationUser cUser = store.FindByIdAsync(adminUser.Id).Result;
                store.SetPasswordHashAsync(cUser, hashedNewPassword);
                store.UpdateAsync(cUser);

                Console.WriteLine("Password has been reset... hit any key");
                Console.Read();
            }
        }

        public static string SolicitPassword(string prompt)
        {
            string pass = "";
            Console.Write(prompt);
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            //Console.WriteLine("The Password You entered is : " + pass);
            return pass;
        }
    }
}

