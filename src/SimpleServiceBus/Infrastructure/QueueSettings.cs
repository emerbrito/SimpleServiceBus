using SimpleServiceBus.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Infrastructure
{
    class QueueSettings
    {

        public IMessageFormatter Formatter { get; set; }
        public string Label { get; set; }
        public long MaxJournalQueueSize { get; set; }
        public long MaxQueueSize { get; set; }
        public string Path { get; set; }
        public bool Transactional { get; set; }
        public bool UseConnectionCache { get; set; }
        public bool UseJournalQueue { get; set; }
        public bool DenySharedReceive { get; set; }

        internal void Validate()
        {

            string msg;

            if(string.IsNullOrWhiteSpace(this.Path))
            {
                msg = "Queue path is required in order to create or retrieve a queue.";
                throw new QueueBuilderValidationException(msg);
            }

            if (this.Path.Length > 124)
            {
                msg = $"Queue path exceeds the maximun length of 124 characteres. Current path: {this.Path}";
                throw new QueueBuilderValidationException(msg);
            }

        }

    }
}
