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

        public IPublisherBuilder<T> SetAdditionalQueue(string queueName)
        {

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(SetAdditionalQueue));

            if (!settings.AdditionalQueues.Contains(queueName))
                settings.AdditionalQueues.Add(queueName);

            return this;

        }

        public IPublisherBuilder<T> AutoCreateLocalQueues()
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
            List<MessageQueue> extraQueues = new List<MessageQueue>();

            foreach (var name in settings.AdditionalQueues)
            {
                extraQueues.Add(GetMessageQueue(name));
            }


            if (settings.Logger == null)
            {
                ILogManager lmanager = (logManager != null) ? logManager : new DefaultLogManager();
                settings.Logger = lmanager.GetLogger(typeof(Subscriber));
            }

            var publisher = new Publisher<T>(queue, extraQueues, settings);
            return publisher;

        }

        private MessageQueue GetMessageQueue(string queueName)
        {


            MessageQueue queue;
            IQueueBuilder builder;

            var queuePath = QueueBuilder.TryFormatPath(queueName);

            builder = QueueBuilder.New(queuePath)
                .WithJsonSerialization();

            if (settings.UseAmbientTransactions)
                builder.AsTransactional();


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
