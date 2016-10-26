using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Abstractions
{
    public interface ISubscriber
    {

        SubscriberStatus Status { get; }

        void Start();
        void Stop();   

    }
}
