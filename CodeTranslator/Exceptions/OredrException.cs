using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeTranslator.Exceptions
{
    public class OredrException : Exception
    {
        public OredrException() : base() { }
        public OredrException(string exception, int line, params object[] args)
            : base(string.Format(exception, line, args[0])) { }
        public OredrException(string exceptionMessage, Exception inner) : base(exceptionMessage, inner) { }
    }
}
