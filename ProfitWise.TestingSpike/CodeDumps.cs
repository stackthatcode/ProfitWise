using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Push.Utilities.Helpers;

namespace ProfitWise.Batch
{
    class CodeDumps
    {

        public static void TestNumberOfPages()
        {
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 0));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 1));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 10));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 11));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 20));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 21));
            Console.WriteLine(PagingFunctions.NumberOfPages(10, 30));
            Console.ReadLine();
            // 0 / 10 = 0
            // 1 / 10 = 1
            // 10 / 10 = 1
            // 11 / 10 = 2
            // 20 / 10 = 2
            // 21 / 10 = 3            
        }


        // Performance Testing Scraps of Code
 
        public static void MySqlTesting()
        {
            var connectionstring = "server=127.0.0.1;uid=root;pwd=sqlBoomba123!@#;database=profitwise;";
            var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionstring);
            connection.Open();

            //var query = "SELECT * FROM OrderSkuHistory";
            //var query = @"SELECT * FROM OrderSkuHistory WHERE LineId > 10000 AND LineId < 10043";
            var query = @"INSERT INTO OrderSkuHistory (OrderNumber, ProductSku, Price, CoGS) VALUES ('1312381930', 'ABCDEFG001', 9.00, 7.50);";

            Console.WriteLine("Start... ");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            connection.Execute("START TRANSACTION;");

            var counter = 0;
            while (counter < 50000)
            {
                //Console.WriteLine(DateTime.Now+ " " + counter);
                var result = connection.Execute(query);
                counter++;
            }

            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = 
                String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            connection.Execute("COMMIT;");
            Console.WriteLine("End... " + elapsedTime);
            Console.ReadLine();
        }
        

    }
}
