using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Exceptions
{
    public class QueueNotFoundException : Exception
    {

        public string Path { get; set; }

        public QueueNotFoundException(string path)
        {
            Path = path;
        }

        public QueueNotFoundException(string path, string message)
            : base(message)
        {
            Path = path;
        }

    }
}
