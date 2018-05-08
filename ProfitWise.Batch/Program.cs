﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Hangfire;
using Microsoft.AspNet.Identity.EntityFramework;
using ProfitWise.Data.Factories;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Processes;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using Push.Foundation.Web.Identity;


namespace ProfitWise.Batch
{
    class Program
    {
        private const string HangFireBackgroundServiceOption = "1";
        private const string ScheduleInitialShopRefreshOption = "2";
        private const string ScheduleBackgroundSystemJobsOption = "3";
        private const string EmitTimeZones = "4";
        private const string ManualShopRefresh = "5";
        private const string PopulateCalendar = "6";
        private const string SingleExchangeRateLoad = "7";
        private const string AllExchangeRateLoad = "8";
        private const string RefreshSingleOrder = "9";
        private const string RebuildOrderLineCogsAndLedger = "10";

        private const string AppUninstallTestRequest = "UNI";
        private const string ResetAdminPasswordOption = "W";
        

        private static string SolicitUserInput()
        {
            Console.WriteLine("Please select from the following options and hit enter");
            Console.WriteLine($"{HangFireBackgroundServiceOption} - Run HangFire Background Service");
            Console.WriteLine($"{ScheduleInitialShopRefreshOption} - Schedule an Initial Shop Refresh");
            Console.WriteLine($"{ScheduleBackgroundSystemJobsOption} - Schedule the default ProfitWise System Jobs");
            Console.WriteLine($"{EmitTimeZones} - Save all Time Zone Id's on the current machine to a text file");
            Console.WriteLine($"{ManualShopRefresh} - Execute a manual Shop Refresh");
            Console.WriteLine($"{PopulateCalendar} - Populate the SQL calendar_table");
            Console.WriteLine($"{SingleExchangeRateLoad} - Import Exchange Rate for a specific Currency");
            Console.WriteLine($"{AllExchangeRateLoad} - Import Complete Exchange Rate data set");
            Console.WriteLine($"{RefreshSingleOrder} - Refresh a Single Order");
            Console.WriteLine($"{RebuildOrderLineCogsAndLedger} - Rebuild Order Line Unit CoGS and the Report Ledger for a single Shop");

            Console.WriteLine("");
            return Console.ReadLine();
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("ProfitWise.Batch v1.0 - Push Automated Commerce LLC");
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("");

            var choice =
                args.Length > 0 && args[0].ToLower() == "service"
                    ? HangFireBackgroundServiceOption
                    : SolicitUserInput()?.Trim();

            if (choice == HangFireBackgroundServiceOption)
            {
                HangFireBackgroundService();
                return;
            }
            if (choice == ScheduleBackgroundSystemJobsOption)
            {
                ScheduleBackgroundSystemJobs();
                return;
            }
            if (choice == ScheduleInitialShopRefreshOption)
            {
                ScheduleInitialShopRefresh();
                return;
            }
            if (choice == ResetAdminPasswordOption)
            {
                ResetAdminPassword();
                return;
            }
            if (choice == AppUninstallTestRequest)
            {
                AppUninstallTest.Execute();
                return;
            }
            if (choice == EmitTimeZones)
            {
                EmitTimeZonesToConsoleAndTextFile();
                return;
            }
            if (choice == ManualShopRefresh)
            {
                ExecuteManualShopRefresh();
                return;
            }
            if (choice == PopulateCalendar)
            {
                PopulateCalendarProcess();
                return;
            }
            if (choice == SingleExchangeRateLoad)
            {
                LoadNewCurrencyAndExchangeRates();
                return;
            }
            if (choice == AllExchangeRateLoad)
            {
                RunExchangeRateLoad();
                return;
            }
            if (choice == RefreshSingleOrder)
            {
                RunRefreshSingleOrder();
                return;
            }
            if (choice == RebuildOrderLineCogsAndLedger)
            {
                RunRebuildOrderLineCogsAndLedger();
                return;
            }

            Console.WriteLine("Invalid option - exiting. Bye!");
        }

        private static void RunRebuildOrderLineCogsAndLedger()
        {
            Console.WriteLine($"Enter Shop Id");
            var shopId = Int32.Parse(Console.ReadLine());

            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var repository = scope.Resolve<ShopRepository>();
                var factory = scope.Resolve<MultitenantFactory>();
                var shop = repository.RetrieveByShopId(shopId);
                var service = factory.MakeCogsService(shop);

                service.RecomputeCogsFullDatalog();
            }

            ExitWithAnyKey();
        }

        private static void RunRefreshSingleOrder()
        {
            Console.WriteLine($"Enter Shop Id");
            var shopId = Int32.Parse(Console.ReadLine());

            Console.WriteLine($"Enter Shopify Order Id");
            var orderId = long.Parse(Console.ReadLine());
            
            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var repository = scope.Resolve<ShopRepository>();
                var shop = repository.RetrieveByShopId(shopId);

                var process = scope.Resolve<ShopRefreshProcess>();
                process.RefreshSingleOrder(shop.ShopOwnerUserId, orderId);
            }

            ExitWithAnyKey();
        }


        private static void RunExchangeRateLoad()
        {
            Console.WriteLine(
                $"Are you sure you want to run this process? If so, type 'YES'");
            var confirm = Console.ReadLine();
            if (confirm != "YES")
            {
                return;
            }

            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var currencyService = scope.Resolve<ExchangeRateRefreshProcess>();
                currencyService.Execute();
            }
            ExitWithAnyKey();
        }


        private static void LoadNewCurrencyAndExchangeRates()
        {
            Console.WriteLine("NOTE: the new Currency must already be loaded in the 'currency' table");
            Console.WriteLine("Enter the Currency Symbol e.g. USD");
            var symbol = Console.ReadLine();
            Console.WriteLine("Enter Start Date e.g. 2008-03-01");
            var date = Console.ReadLine();

            Console.WriteLine($"Are you sure you want to run this process for '{symbol}'? Type 'YES' if so.");
            var confirm = Console.ReadLine();
            if (confirm != "YES")
            {
                return;
            }

            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var currencyService = scope.Resolve<ExchangeRateRefreshProcess>();
                currencyService.LoadNewCurrency(symbol, DateTime.Parse(date));
            }
            ExitWithAnyKey();
        }
        
        public static void ExecuteManualShopRefresh()
        {
            Console.WriteLine("Please enter the Shop Owner's User Id");
            string userId = Console.ReadLine();
            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<ShopRefreshProcess>();
                service.RoutineShopRefresh(userId);
            }
            ExitWithAnyKey();
        }

        public static void EmitTimeZonesToConsoleAndTextFile()
        {
            var outputFileName = "timezones.txt";

            Console.WriteLine($"Emitting all Time Zone Id's from current machine to {outputFileName}...");
            var output = new List<string>();
            foreach (TimeZoneInfo z in TimeZoneInfo.GetSystemTimeZones())
            {
                output.Add(z.Id);
            }
            System.IO.File.WriteAllLines(outputFileName, output);
            ExitWithAnyKey();
        }


        public static void ExitWithAnyKey()
        {
            Console.WriteLine("Hit any key to exit... FIN");
            Console.ReadKey();
        }


        public static void ScheduleBackgroundSystemJobs()
        {
            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<HangFireService>();
                service.AddOrUpdateExchangeRateRefresh();
                service.AddOrUpdateSystemCleanupProcess();

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


        public static void PopulateCalendarProcess()
        {
            var container = Bootstrapper.ConfigureApp(false);
            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<CalendarPopulationService>();
                service.Execute();
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

