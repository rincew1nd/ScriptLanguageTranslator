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
            Assert.AreEqual(246, CallCalc("CalculateNumber", "123 + 123"));
            Assert.AreEqual(81, CallCalc("CalculateNumber", "123 - 42"));
            Assert.AreEqual(369, CallCalc("CalculateNumber", "123 * 3"));
            Assert.AreEqual(61.5, CallCalc("CalculateNumber", "123 / 2"));
        }

        [Test]
        public void BooleanCalculation()
        {
            Assert.AreEqual(1, CallCalc("CalculateNumber", "1 | 0"));
            Assert.AreEqual(0, CallCalc("CalculateNumber", "!1 | 0"));
            Assert.AreEqual(0, CallCalc("CalculateNumber", "1 & 0"));
            Assert.AreEqual(1, CallCalc("CalculateNumber", "1 & 1"));
            Assert.AreEqual(1, CallCalc("CalculateNumber", "1 & !0"));
        }

        [Test]
        public void AlotOfBrackets()
        {
            CallCalc("ParseOperation", "((12-43*(3-1)-10)-12)-34+34-(12/3)");
        }

        public object CallCalc(string method, params object[] text)
        {
            var translator = new Translator();
            var mi = translator.GetType().GetMethod(
                "CalculateNumber", BindingFlags.NonPublic | BindingFlags.Instance);
            return mi != null ? mi.Invoke(translator, text) : null;
        }
    }
}
