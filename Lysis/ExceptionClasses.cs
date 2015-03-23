using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourcePawn;

namespace Lysis
{
    public class OpCodeNotKnownException : Exception
    {
        public OpCodeNotKnownException(string message) : base(message)
        {
        }
    }

    public class LogicChainConversionException : Exception
    {
        public LogicChainConversionException(string message) : base (message)
        {
        }
    }
}
