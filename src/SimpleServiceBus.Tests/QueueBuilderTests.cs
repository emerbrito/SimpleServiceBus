using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleServiceBus.Exceptions;
using SimpleServiceBus.Serialization;
using SimpleServiceBus.Infrastructure;

namespace SimpleServiceBus.Tests
{
    [TestClass]
    public class QueueBuilderTests
    {

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuilderConfigNullPathValidation()
        {

            var qbuilder = (QueueBuilder) QueueBuilder.New(string.Empty);
            qbuilder.settings.Validate();

        }

        [TestMethod]
        public void BuilderConfigAllSettings()
        {
            
            string path = @".\Private$\MyQueue";
            string desc = "Queue description";
            var qbuilder = (QueueBuilder) QueueBuilder.New("MyQueue");
            QueueSettings config;

            qbuilder
                .AsTransactional()
                .UseConnectionCache()
                .WithExclusiveReadAccess()
                .UseJournalQueue()
                .WithDescription(desc)
                .WithFormatter(new JsonFormatter())
                .WithMaxJournalSize(1000)
                .WithMaxQueueSize(1000);

            config = qbuilder.settings;

            Assert.AreEqual(typeof(JsonFormatter), config.Formatter.GetType(), "Formatter");
            Assert.AreEqual(desc, config.Label, "Label");
            Assert.AreEqual(true, config.UseConnectionCache, "UseConnectionCache");
            Assert.AreEqual(true, config.DenySharedReceive, "DenySharedReceive");
            Assert.AreEqual(1000, config.MaxJournalQueueSize, "MaxJournalQueueSize");
            Assert.AreEqual(1000, config.MaxQueueSize, "MaxQueueSize");
            Assert.AreEqual(path, config.Path, "Path");
            Assert.AreEqual(true, config.Transactional, "Transactional");
            Assert.AreEqual(true, config.UseJournalQueue, "UseJournalQueue");

        }

        [TestMethod]
        public void BuilderConfigFormatterType()
        {

            string path = @".\Private$\MyQueue";
            var qbuilder = (QueueBuilder) QueueBuilder.New(path);
            QueueSettings config;

            qbuilder.WithFormatter<JsonFormatter>();
            config = qbuilder.settings;

            Assert.AreEqual(typeof(JsonFormatter), config.Formatter.GetType(), "Formatter");

        }

        [TestMethod]
        public void BuilderConfigJsonFormatter()
        {

            string path = @".\Private$\MyQueue";
            var qbuilder = (QueueBuilder) QueueBuilder.New(path);
            QueueSettings config;

            qbuilder.WithJsonSerialization();
            config = qbuilder.settings;

            Assert.AreEqual(typeof(JsonFormatter), config.Formatter.GetType(), "Formatter");

        }

        [TestMethod]
        public void BuilderConfigDefaultFormatter() 
        {

            string path = @".\Private$\MyQueue";
            var qbuilder = (QueueBuilder) QueueBuilder.New(path);
            QueueSettings config;

            config = qbuilder.settings;

            Assert.IsNull(config.Formatter, "Formatter");

        }

    }
}
