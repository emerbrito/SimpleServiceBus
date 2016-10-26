using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Abstractions
{
    public interface ISubscriptionBuilder 
    {

        ISubscriptionBuilder SetMessageHandler<TMessage>(Action<TMessage> messageHandler);

        ISubscriptionBuilder MaxAttemptsOnFailure(int attempts);

        ISubscriptionBuilder AutoCreateLocalQueues();

        ISubscriptionBuilder PauseAfterFailedAttempts(int secondsToPause);

        ISubscriptionBuilder WithErrorQueue(string queueName);

        ISubscriptionBuilder WithLogger(ILogger logger);

        ISubscriptionBuilder DequeueBeforeHandling();

        ISubscriber Create();

    }
}
