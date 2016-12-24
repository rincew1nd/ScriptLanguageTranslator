using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CodeTranslator.Exceptions
{
    public class OredrException : Exception
    {
        public OredrException() : base() { }

        public OredrException(string exception, int line, params object[] args)
            : base(string.Format(exception, line, args[0]))
        {
            Utils.SelectText(line, args[0].ToString());
        }

        public OredrException(string exceptionMessage, Exception inner) : base(exceptionMessage, inner) { }
    }
}
