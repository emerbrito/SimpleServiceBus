using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Infrastructure
{
    public interface IQueueBuilder 
    {

        IQueueBuilder AsTransactional();
        IQueueBuilder WithJsonSerialization();
        IQueueBuilder WithFormatter(IMessageFormatter formatterInstance);
        IQueueBuilder WithFormatter<T>() where T : IMessageFormatter;
        IQueueBuilder WithDescription(string description);
        IQueueBuilder UseConnectionCache();
        IQueueBuilder UseJournalQueue();
        IQueueBuilder WithExclusiveReadAccess();
        IQueueBuilder WithMaxJournalSize(long maxSize);
        IQueueBuilder WithMaxQueueSize(long maxSize);
        
        MessageQueue Create();

        MessageQueue TryCreate();

        MessageQueue Retrieve();

    }
}
