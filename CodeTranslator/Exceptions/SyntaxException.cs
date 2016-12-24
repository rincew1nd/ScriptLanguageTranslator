using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CodeTranslator.Exceptions
{
    public class SyntaxException : Exception
    {
        public SyntaxException() : base() { }

        public SyntaxException(string exception, int line, params object[] args)
            : base(string.Format(exception, line, args[0]))
        {
            Utils.SelectText(line, args[0].ToString());
        }

        public SyntaxException(string exceptionMessage, Exception inner) : base(exceptionMessage, inner) { }
    }
}
