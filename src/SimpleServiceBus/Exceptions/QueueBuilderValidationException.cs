using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Exceptions
{
    public class QueueBuilderValidationException : Exception
    {

        public QueueBuilderValidationException(string message)
            : base(message)
        {
        }

    }
}
