using SimpleServiceBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus
{
    public class PublisherSettings
    {

        public string QueueName { get; set; }

        public List<string> AdditionalQueues { get; set; } = new List<string>();

        public bool CreateLocalQueues { get; set; }

        public bool UseAmbientTransactions { get; set; }

        public ILogger Logger { get; set; }

    }
}
