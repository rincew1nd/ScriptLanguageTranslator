using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CodeTranslator;

namespace UnitTests
{
    static class Utils
    {
        public static object PrivateMethod(this object obj, string method, params object[] text)
        {
            var mi = obj.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            return mi != null ? mi.Invoke(obj, text) : null;
        }
    }
}
