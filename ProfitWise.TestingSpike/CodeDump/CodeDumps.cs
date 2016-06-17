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

        
        

    }
}
