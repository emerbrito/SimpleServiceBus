using SimpleServiceBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using SimpleServiceBus.Serialization;
using SimpleServiceBus.Exceptions;
using System.Text.RegularExpressions;

namespace SimpleServiceBus.Infrastructure
{
    public class QueueBuilder : IQueueBuilder
    {

        const string ipAddressPattern = "(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        internal readonly QueueSettings settings;

        public static IQueueBuilder New(string queueName)
        {

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName));

            queueName = TryFormatPath(queueName);

            return new QueueBuilder(queueName);

        }

        internal QueueBuilder(string path)
        {
            settings = new QueueSettings();
            settings.Path = path;
        }

        public IQueueBuilder AsTransactional()
        {
            settings.Transactional = true;
            return this;
        }

        public MessageQueue Create()
        {
            return CreateOrRetrieveQueue(tryCreate: false);
        }

        public MessageQueue TryCreate()
        {
            return CreateOrRetrieveQueue(tryCreate: true);
        }

        public MessageQueue Retrieve()
        {
            return RetrieveQueue();
        }

        public IQueueBuilder UseConnectionCache()
        {
            settings.UseConnectionCache = true;
            return this;
        }

        public IQueueBuilder UseJournalQueue()
        {
            settings.UseJournalQueue = true;
            return this;
        }

        public IQueueBuilder WithDescription(string description)
        {
            settings.Label = description;
            return this;
        }

        public IQueueBuilder WithExclusiveReadAccess()
        {
            settings.DenySharedReceive = true;
            return this;
        }

        public IQueueBuilder WithFormatter(IMessageFormatter formatterInstance)
        {

            if (formatterInstance == null)
            {
                throw new ArgumentNullException(
                    nameof(formatterInstance),
                    $"Parameter {nameof(formatterInstance)} cannot be null."
                    );
            }

            settings.Formatter = formatterInstance;
            return this;

        }

        public IQueueBuilder WithFormatter<T>() where T : IMessageFormatter
        {
            settings.Formatter = Activator.CreateInstance<T>();
            return this;
        }

        public IQueueBuilder WithJsonSerialization()
        {
            settings.Formatter = new JsonFormatter();
            return this;
        }

        public IQueueBuilder WithMaxJournalSize(long maxSizeKB)
        {
            settings.MaxJournalQueueSize = maxSizeKB;
            return this;
        }

        public IQueueBuilder WithMaxQueueSize(long maxSizeKB)
        {
            settings.MaxQueueSize = maxSizeKB;
            return this;
        }

        private bool QueueExists(string path)
        {

            if (string.IsNullOrWhiteSpace(nameof(path)))
                throw new ArgumentNullException(nameof(path));

            if (MessageQueue.Exists(path))
            {
                return true;
            }

            return false;

        }

        private MessageQueue CreateOrRetrieveQueue(bool tryCreate)
        {

            settings.Validate();
            MessageQueue queue;

            if(QueueExists(settings.Path))
            {

                if(tryCreate)
                {
                    queue = GetExisting(settings.Path, settings.DenySharedReceive, settings.UseConnectionCache);                    
                }
                else
                {
                    throw new QueueAlreadyExistException(
                        settings.Path,
                        $"Unable to create queue {settings.Path}. Queue already exist. Consider using TryCreate if you want to check and return existing queue when one with is already available."
                        );
                }

            }
            else
            {
                CreateNew(settings.Path, settings.Transactional);
                queue = GetExisting(settings.Path, settings.DenySharedReceive, settings.UseConnectionCache);
            }

            ConfigureQueue(queue);

            return queue;
            
        }

        private MessageQueue RetrieveQueue()
        {

            settings.Validate();
            MessageQueue queue;

            if (QueueExists(settings.Path))
            {

                queue = GetExisting(settings.Path, settings.DenySharedReceive, settings.UseConnectionCache);

            }
            else
            {
                throw new QueueNotFoundException(settings.Path, $"Unable to locate queue: {settings.Path}");
            }

            ConfigureQueue(queue);

            return queue;

        }


        private void CreateNew(string path, bool transactional)
        {
            using (MessageQueue queue = MessageQueue.Create(path, transactional))
            {
                queue.SetPermissions(Environment.UserDomainName + "\\" + Environment.UserName, MessageQueueAccessRights.FullControl);
                queue.SetPermissions("Administrators", MessageQueueAccessRights.FullControl);
                queue.SetPermissions("Everyone", MessageQueueAccessRights.GenericRead);
                queue.SetPermissions("Everyone", MessageQueueAccessRights.GenericWrite);
            }
        }

        private MessageQueue GetExisting(string path, bool denySharedReceive, bool enableCache)
        {
            var mq = new MessageQueue(
                path, 
                sharedModeDenyReceive: denySharedReceive, 
                enableCache: enableCache );

            ValidateExisting(mq);

            return mq;

        }

        private void ValidateExisting(MessageQueue queue)
        {

            if(settings.Transactional && !queue.Transactional)
            {
                throw new QueueBuilderValidationException($"The current configuration requires a transactional queue but the existing queue retrieved by {nameof(QueueBuilder)} is non-transaction.");
            }

            queue.Formatter = settings.Formatter;

        }

        private void ConfigureQueue(MessageQueue queue)
        {
            
            queue.DenySharedReceive = settings.DenySharedReceive;
            queue.Formatter = settings.Formatter;            
            queue.UseJournalQueue = settings.UseJournalQueue;

            if(!string.IsNullOrWhiteSpace(settings.Label))
                queue.Label = settings.Label;

            if (settings.UseJournalQueue && settings.MaxJournalQueueSize > 0)
                queue.MaximumJournalSize = settings.MaxJournalQueueSize;

            if (settings.MaxQueueSize > 0)
                queue.MaximumQueueSize = settings.MaxQueueSize;

        }

        internal static string TryFormatPath(string queue)
        {

            if (!queue.Contains("\\") && !queue.Contains("@"))
            {
                queue = $@"private$\{queue}";
            }

            if (IsPrivateQueuePath(queue))
            {
                if (string.IsNullOrEmpty(GetMachineNameFromPath(queue)))
                {
                    queue = $@".\{queue}";
                }
            }

            return queue;

        }

        internal static string GetMachineNameFromPath(string queuePath)
        {


            string machineName = string.Empty;

            if (queuePath.StartsWith(Environment.MachineName))
            {
                return ".";
            }

            if (queuePath.Contains("@"))
            {
                return queuePath.Substring(queuePath.IndexOf('@') + 1);
            }
                
            if (queuePath.Contains("\\"))
            {
                machineName = queuePath.Substring(0, queuePath.IndexOf("\\"));
                if(machineName.ToLower().Contains("private$"))
                {
                    machineName = string.Empty;
                }
            }

            return machineName;

        }

        internal static string GetQueueNameFromPath(string queuePath)
        {

            string queueName = queuePath;

            if (queuePath.EndsWith("\\"))
            {
                queuePath = queuePath.Substring(0, queuePath.Length - 1);
            }

            if (queuePath.Contains("@"))
            {
                queueName = queuePath.Substring(0, queuePath.IndexOf('@'));
            }

            if (queuePath.Contains("\\"))
            {
                int index = queuePath.LastIndexOf("\\") + 1;
                queueName = queuePath.Substring(index, queuePath.Length - index);
            }

            return queueName;

        }

        internal static bool IsPrivateQueuePath(string queuePath)
        {

            if(queuePath.ToLower().Contains("private$"))
            {
                return true;
            }

            return false;

        }

    }
}
