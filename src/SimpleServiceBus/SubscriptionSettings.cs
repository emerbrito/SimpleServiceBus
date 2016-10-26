using SimpleServiceBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus
{
    class SubscriptionSettings
    {

        public string QueueName { get; set; }
        public string ErrorQueueName { get; set; }
        public int AttemptsBeforeFail { get; set; } = 1;
        public Delegate MessageHandler { get; set; }
        public bool AutoCreateLocalQueues { get; set; }
        public bool PauseOnError { get; set; }
        public int SecondsToPause { get; set; }
        public ILogger Logger { get; set; }
        public bool DequeueBeforeHandling { get; set; }

        public void Validate()
        {

            if (string.IsNullOrWhiteSpace(QueueName))
            {
                throw new ArgumentException("A read queue is required to create a subscription.");
            }

            if (MessageHandler == null)
            {
                throw new ArgumentException($"Message handler cannot be null.");
            }

        }

    }
}
