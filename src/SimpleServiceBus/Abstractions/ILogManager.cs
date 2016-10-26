using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Abstractions
{
    public interface ILogManager
    {

        ILogger GetLogger(Type type);

    }
}
