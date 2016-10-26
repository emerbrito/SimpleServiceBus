using SimpleServiceBus.Abstractions;
using SimpleServiceBus.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SimpleServiceBus
{
    public class Publisher<T> : IPublisher<T>
    {

        private readonly ILogger log;
        private readonly MessageQueue mqueue;
        private readonly PublisherSettings settings;

        internal Publisher(MessageQueue mqueue, PublisherSettings settings)
        {
            this.log = settings.Logger;
            this.mqueue = mqueue;
            this.settings = settings;
        }

        public void Send(T message)
        {
            Send(message, label: string.Empty);
        }

        public void Send(T message, string label)
        {

            
            var qmsg = new Message(message, new JsonFormatter());
            if (!string.IsNullOrWhiteSpace(label))
            {
                qmsg.Label = label;
            }

            if (mqueue.Transactional)
            {
                
                if (settings.UseAmbientTransactions)
                {
                    if (Transaction.Current != null)
                    {
                        log.Trace($"Sending to transactional queue: {mqueue.Path}. Type: Automatic. Message label: {label}");
                        mqueue.Send(qmsg, MessageQueueTransactionType.Automatic);
                    }
                    else
                    {
                        log.Trace($"Sending to transactional queue: {mqueue.Path}. Type: Single. Message label: {label}");
                        mqueue.Send(qmsg, MessageQueueTransactionType.Single);
                    }
                }
                else
                {
                    log.Trace($"Sending to transactional queue: {mqueue.Path}. No ambient transaction required Type: Single. Message label: {label}");
                    mqueue.Send(qmsg, MessageQueueTransactionType.Single);
                }

            }
            else
            {
                log.Trace($"Sending to non transactional queue: {mqueue.Path}. Message label: {label}");
                mqueue.Send(qmsg);
            }

        }

    }
}
