using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ProfitWise.Test
{
    [TestFixture]
    public class GenericTests
    {
        [Test]
        public void PlayWithStrings()
        {
            var input = "Abc this   is    my test";
            var terms =
                input.Split(' ')
                    .Select(x => x.Trim())
                    .Where(x => x != "")
                    .ToList();

        }

    }
}

