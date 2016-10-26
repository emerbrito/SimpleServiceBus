using SimpleServiceBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Infrastructure
{
    class DefaultLogManager : ILogManager
    {
        public ILogger GetLogger(Type type)
        {
            return new DefaultLogger();
        }
    }
}
