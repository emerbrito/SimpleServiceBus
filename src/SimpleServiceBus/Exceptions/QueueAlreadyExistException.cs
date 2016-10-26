using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Exceptions
{
    public class QueueAlreadyExistException : Exception
    {

        public string Path { get; set; }

        public QueueAlreadyExistException(string path)
        {
            Path = path;
        }

        public QueueAlreadyExistException(string path, string message)
            : base(message)
        {
            Path = path;
        }

    }
}
