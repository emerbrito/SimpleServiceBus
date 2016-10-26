using SimpleServiceBus.Abstractions;
using SimpleServiceBus.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus
{
    public class ServiceFactory
    {


        public static ISubscriptionBuilder NewSubscription(string queueName)
        {
            var builder = new SubscriptionBuilder(queueName);
            return builder;
        }

        public static IPublisherBuilder<TMessage> NewPublisher<TMessage>(string queueName)
        {
            var builder = new PublisherBuilder<TMessage>(queueName);
            return builder;
        }

    }
}
