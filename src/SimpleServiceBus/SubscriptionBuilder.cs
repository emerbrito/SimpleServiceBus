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
    class SubscriptionBuilder : ISubscriptionBuilder
    {

        internal readonly SubscriptionSettings settings;
        internal ILogManager logManager;

        internal SubscriptionBuilder(string queueName)
        {

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName));

            settings = new SubscriptionSettings();
            settings.QueueName = queueName;

        }

        public ISubscriptionBuilder SetMessageHandler<TMessage>(Action<TMessage> messageHandler)
        {
            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler));

            settings.MessageHandler = messageHandler;
            return this;

        }

        public ISubscriptionBuilder MaxAttemptsOnFailure(int attempts)
        {

            if (attempts < 1)
                throw new ArgumentOutOfRangeException(nameof(attempts), $"Value of {nameof(MaxAttemptsOnFailure)} should be greather than one.");

            settings.AttemptsBeforeFail = attempts;
            return this;
        }

        public ISubscriptionBuilder AutoCreateLocalQueues()
        {
            settings.AutoCreateLocalQueues = true;
            return this;
        }

        public ISubscriptionBuilder PauseAfterFailedAttempts(int seconds)
        {

            if (seconds < 1)
                throw new ArgumentOutOfRangeException(nameof(seconds), $"Value of {nameof(PauseAfterFailedAttempts)} should be greather than one.");

            settings.PauseOnError = true;
            settings.SecondsToPause = seconds;
            return this;

        }

        public ISubscriptionBuilder WithErrorQueue(string queueName)
        {

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName));

            settings.ErrorQueueName = queueName;
            return this;
        }

        public ISubscriptionBuilder WithLogger(ILogger logger)
        {

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            settings.Logger = logger;
            return this;

        }

        public ISubscriptionBuilder DequeueBeforeHandling()
        {
            settings.DequeueBeforeHandling = true;
            return this;
        }

        public ISubscriber Create()
        {

            settings.Validate();
            return CreateSubscription();

        }

        private ISubscriber CreateSubscription()
        {

            ISubscriber subscriber = null;
            MessageQueue readQueue = null;
            MessageQueue errorQueue = null;

            if(settings.Logger == null)
            {
                ILogManager lmanager = (logManager != null) ? logManager : new DefaultLogManager();
                settings.Logger = lmanager.GetLogger(typeof(Subscriber));
            }

            if(settings.PauseOnError && !string.IsNullOrWhiteSpace(settings.ErrorQueueName))
            {
                throw new ArgumentException($"Invalid configuration. Properties {nameof(PauseAfterFailedAttempts)} and nameof{nameof(WithErrorQueue)} are mutually exclusive. If the subscriber is set to pause and retry, the failed message will never be sent to an error queue.");
            }

            readQueue = GetMessageQueue(settings.QueueName, denySharedReceive: true);

            if (!string.IsNullOrWhiteSpace(settings.ErrorQueueName))
            {
                errorQueue = GetMessageQueue(settings.ErrorQueueName, denySharedReceive: false);
            }

            subscriber = new Subscriber(settings, readQueue, errorQueue, settings.Logger);

            return subscriber;

        }

        private MessageQueue GetMessageQueue(string queueName, bool denySharedReceive)
        {


            MessageQueue queue;
            var queuePath = QueueBuilder.TryFormatPath(queueName);

            IQueueBuilder builder = QueueBuilder.New(queuePath)
                .WithExclusiveReadAccess()
                .WithJsonSerialization();

            if (denySharedReceive)
                builder.WithExclusiveReadAccess();

            if ((QueueBuilder.IsPrivateQueuePath(queuePath) && settings.AutoCreateLocalQueues))
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
