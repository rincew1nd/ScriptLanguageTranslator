using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeTranslator.Exceptions
{
    public class SyntaxException : Exception
    {
        public SyntaxException() : base() { }

        public SyntaxException(string exception, int line, params object[] args)
            : base(string.Format(exception, line, args[0])) { }

        public SyntaxException(string exceptionMessage, Exception inner) : base(exceptionMessage, inner) { }
    }
}
