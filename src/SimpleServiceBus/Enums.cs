using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus
{

    [Flags]
    public enum SubscriberStatus
    {
        Stopped = 1,
        Stopping = 2,
        Started = 4,
        Starting = 8,
        Paused = 16
    }

}
