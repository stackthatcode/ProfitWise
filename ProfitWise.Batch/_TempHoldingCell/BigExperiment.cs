using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitWise.Batch
{
    public class BigExperiment
    {
        public void Run()
        {
            var counter = 0;
            var size = 100000;
            var numberOfKeys = 10000;
            var totals = new Envelope[size];
            var keys = new Dictionary<int, int>();

            while (counter < numberOfKeys)
            {
                keys[counter++] = counter;
            }


            var rnd = new Random();
            
            Console.WriteLine(DateTime.Now + " " + DateTime.Now.Millisecond);
            counter = 0;
            while (++counter < size)
            {
                totals[counter] = new Envelope {Amount = rnd.Next(0, 1000)};
            }

            Console.WriteLine(DateTime.Now + " " + DateTime.Now.Millisecond);
            var runningTotal = 0m;
            counter = 0;
            while (++counter < size)
            {
                var keyValue = keys[counter % numberOfKeys];
                runningTotal += totals[counter].Amount - keyValue;
            }
            Console.WriteLine(DateTime.Now + " " + DateTime.Now.Millisecond);

            Console.ReadLine();

        }
    }


    public class Envelope
    {
        public int Key { get; set; }
        public decimal Amount { get; set; }
    }
}
