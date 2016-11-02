using SimpleServiceBus.Abstractions;
using SimpleServiceBus.Extensions;
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
        private readonly MessageQueue mainQueue;
        private IEnumerable<MessageQueue> extraQueues;
        private readonly PublisherSettings settings;

        internal Publisher(MessageQueue mqueue, IEnumerable<MessageQueue> extraQueues, PublisherSettings settings)
        {

            this.log = settings.Logger;
            this.mainQueue = mqueue;
            this.extraQueues = extraQueues;
            this.settings = settings;

            if (extraQueues == null)
                extraQueues = Enumerable.Empty<MessageQueue>();

        }

        public void Send(T message)
        {
            Send(message, label: string.Empty, pattern: "*");
        }

        public void Send(T message, string label)
        {
            Send(message, label, pattern: "*");
        }

        public void Send(T message, string label, string pattern)
        {


            if (string.IsNullOrWhiteSpace(pattern)) pattern = "*";

            var queueMessage = new Message(message, new JsonFormatter());
            var matchingQueues = new List<MessageQueue>();

            if (!string.IsNullOrWhiteSpace(label))
            {
                queueMessage.Label = label;
            }

            foreach (var q in matchingQueues)
            {
                Send(queueMessage, q);
            }

        }

        void Send(Message message, MessageQueue mqueue)
        {

            if (mqueue.Transactional)
            {

                if (settings.UseAmbientTransactions)
                {
                    if (Transaction.Current != null)
                    {
                        log.Trace($"Sending to transactional queue: {mqueue.Path}. Type: Automatic. Message label: {message.Label?.ToString()}");
                        mqueue.Send(message, MessageQueueTransactionType.Automatic);
                    }
                    else
                    {
                        log.Trace($"Sending to transactional queue: {mqueue.Path}. Type: Single. Message label: {message.Label?.ToString()}");
                        mqueue.Send(message, MessageQueueTransactionType.Single);
                    }
                }
                else
                {
                    log.Trace($"Sending to transactional queue: {mqueue.Path}. No ambient transaction required Type: Single. Message label: {message.Label?.ToString()}");
                    mqueue.Send(message, MessageQueueTransactionType.Single);
                }

            }
            else
            {
                log.Trace($"Sending to non transactional queue: {mqueue.Path}. Message label: {message.Label?.ToString()}");
                mqueue.Send(message);
            }

        }

        IEnumerable<MessageQueue> MatchQueues(string pattern)
        {

            var queues = new List<MessageQueue>();

            if (pattern == "*")
            {
                queues.Add(mainQueue);
                queues.AddRange(extraQueues);
            }
            else
            {

                if (mainQueue.QueueName.Like(pattern))
                    queues.Add(mainQueue);

                foreach (var q in extraQueues)
                {

                    if (q.QueueName.Like(pattern))
                        queues.Add(q);

                }

            }

            return queues;

        }


    }
}
