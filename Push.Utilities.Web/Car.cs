using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push.Utilities.Web
{
    public class Car
    {
        public string Name { get; set; }
        public string Engine { get; set; }
        public string Pistons { get; set; }

        public static string CarLaws { get; set; } // Global

        public static Car Factory() // Global function
        {
            return new Car()
            {
                Engine = "V8",
                Pistons = "Aluminum",
            };
        }

        public void Drive()
        {
            CarLaws = "dfdsjfsdf";
        }
    }


    public class Consumer
    {
        public void Consume()
        {
            var instanceOfCar1 = Car.Factory();
            instanceOfCar1.Name = "Ralph";
            instanceOfCar1.Drive();

            var instanceOfCar2 = Car.Factory();
            instanceOfCar2.Name = "Bob";
            instanceOfCar2.Drive();

            Car.CarLaws = "The eternal laws for all Cars!!!";

            Console.WriteLine(instanceOfCar2.Engine);


        }

    }
}
