using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeTranslator;
using NUnit.Framework;

namespace UnitTests
{
    class Syntax
    {
        [Test]
        public void TestSyntax()
        {
            var input = "asdf";
            var output = new Translator().CheckSyntax(input);
        }
    }
}
