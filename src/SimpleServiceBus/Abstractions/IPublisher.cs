using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Abstractions
{

    public interface IPublisher<T>
    {

        void Send(T message);

        void Send(T message, string label);

        void Send(T message, string label, string pattern);

    }

}
