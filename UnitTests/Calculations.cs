using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeTranslator;
using NUnit.Framework;

namespace UnitTests
{
    public class TestSuite
    {
        [Test]
        public void SimpleCalculations()
        {
            var translator = new Translator();
            Assert.AreEqual(246, translator.PrivateMethod("CalculateNumber", "123 + 123"));
            Assert.AreEqual(81, translator.PrivateMethod("CalculateNumber", "123 - 42"));
            Assert.AreEqual(369, translator.PrivateMethod("CalculateNumber", "123 * 3"));
            Assert.AreEqual(61.5, translator.PrivateMethod("CalculateNumber", "123 / 2"));
        }

        [Test]
        public void BooleanCalculation()
        {
            var translator = new Translator();
            Assert.AreEqual(1, translator.PrivateMethod("CalculateNumber", "1 | 0"));
            Assert.AreEqual(0, translator.PrivateMethod("CalculateNumber", "!1 | 0"));
            Assert.AreEqual(0, translator.PrivateMethod("CalculateNumber", "1 & 0"));
            Assert.AreEqual(1, translator.PrivateMethod("CalculateNumber", "1 & 1"));
            Assert.AreEqual(1, translator.PrivateMethod("CalculateNumber", "1 & !0"));
        }

        [Test]
        public void AlotOfBrackets()
        {
            var translator = new Translator();
            Assert.AreEqual(
                translator.PrivateMethod("ParseOperation", "((12-43*(3-1)-10)-12)-34+34-(9/2)"), "-100,5");
            Assert.AreEqual(
                translator.PrivateMethod("ParseOperation", "((12-43*(3-1)-10)-12)-34+34-(8/2)"), "-100");
        }

        [Test]
        public void BooleanAlgebra()
        {
            var translator = new Translator();
            Assert.AreEqual(translator.PrivateMethod("ParseOperation", "!1|0&(1|0)"), "0");
            Assert.AreEqual(translator.PrivateMethod("ParseOperation", "(1&!0)|(0|!1)"), "1");
        }
    }
}
