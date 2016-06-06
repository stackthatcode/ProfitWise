using System;
using System.Linq;
using Dapper;

namespace ProfitWise.TestingSpike
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = ConnectionFactory.Make();
            var query = @"SELECT * FROM OrderSkuHistory WHERE LineId > 1300000 AND LineId < 1300043";


            Console.WriteLine("Starting... " + DateTime.Now);

            var counter = 0;
            while (counter < 1000)
            {
            var result =
                connection
                    .Query<OrderSkuHistory>(query)
                    .ToList();

                Console.WriteLine(DateTime.Now + " " + result.First().Price);
                counter ++;
            }

            Console.WriteLine("End... " + DateTime.Now);

            Console.ReadLine();

        }
    }
}
