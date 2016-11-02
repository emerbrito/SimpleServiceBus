using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Abstractions
{
    public interface IPublisherBuilder<T>
    {

        IPublisherBuilder<T> AutoCreateLocalQueues();

        IPublisherBuilder<T> SetAdditionalQueue(string queueName);

        IPublisherBuilder<T> WithLogger(ILogger logger);

        IPublisherBuilder<T> UseAmbientTransaction();

        IPublisher<T> Create();

    }
}
