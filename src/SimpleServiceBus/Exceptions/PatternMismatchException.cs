using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Exceptions
{
    public class PatternMismatchException : Exception
    {

        public PatternMismatchException(string pattern) : 
            base($"Unable to match pattern {pattern} to an existing queue.")
        {
        }

    }
}
