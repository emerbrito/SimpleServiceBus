using SimpleServiceBus.Abstractions;
using SimpleServiceBus.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus
{
    public class PublisherBuilder<T> : IPublisherBuilder<T>
    {

        internal ILogManager logManager;
        internal readonly PublisherSettings settings;

        internal PublisherBuilder(string queueName)
        {

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName));

            settings = new PublisherSettings();
            settings.QueueName = queueName;

        }

        public IPublisherBuilder<T> AutoCreateLocalQueue()
        {
            settings.CreateLocalQueues = true;
            return this;
        }

        public IPublisherBuilder<T> UseAmbientTransaction()
        {
            settings.UseAmbientTransactions = true;
            return this;
        }

        public IPublisherBuilder<T> WithLogger(ILogger logger)
        {

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            settings.Logger = logger;
            return this;

        }

        public IPublisher<T> Create()
        {

            MessageQueue queue = GetMessageQueue(settings.QueueName);

            if (settings.Logger == null)
            {
                ILogManager lmanager = (logManager != null) ? logManager : new DefaultLogManager();
                settings.Logger = lmanager.GetLogger(typeof(Subscriber));
            }

            var publisher = new Publisher<T>(queue, settings);
            return publisher;

        }

        private MessageQueue GetMessageQueue(string queueName)
        {


            MessageQueue queue;

            var queuePath = QueueBuilder.TryFormatPath(queueName);

            IQueueBuilder builder = QueueBuilder.New(queuePath)
                .WithJsonSerialization();

            if ((QueueBuilder.IsPrivateQueuePath(queuePath) && settings.CreateLocalQueues))
            {
                queue = builder.TryCreate();
            }
            else
            {
                queue = builder.Retrieve();
            }

            return queue;


        }

    }
}
