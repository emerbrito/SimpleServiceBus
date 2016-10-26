using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleServiceBus.Infrastructure;
using SimpleServiceBus.Abstractions;

namespace SimpleServiceBus.Tests
{
    [TestClass]
    public class SubscriptionBuilderTests
    {

        [TestMethod]
        public void SetupWithPauseOnError()
        {

            string readQueue = "myqueue";
            string errorQueue = "myqueue";
            SubscriptionSettings settings;

            var builder = (SubscriptionBuilder) new SubscriptionBuilder(readQueue)
                .WithErrorQueue(errorQueue)
                .WithLogger(new DefaultLogger())
                .AutoCreateLocalQueues()
                .MaxAttemptsOnFailure(5)
                .PauseAfterFailedAttempts(300);

            settings = builder.settings;

            Assert.AreEqual(readQueue, settings.QueueName, "QueueName");
            Assert.AreEqual(errorQueue, settings.ErrorQueueName, "ErrorQueueName");
            Assert.AreEqual(5, settings.AttemptsBeforeFail, "AttemptsBeforeFail");
            Assert.AreEqual(true, settings.AutoCreateLocalQueues, "AutoCreateLocalQueues");
            Assert.IsTrue(settings.Logger.GetType() == typeof(DefaultLogger), "Logger");
            Assert.AreEqual(true, settings.PauseOnError, "PauseOnError");
            Assert.AreEqual(300, settings.SecondsToPause, "SecondsToPause");

        }

        [TestMethod]
        public void SetupWithStopOnError()
        {

            string readQueue = "myqueue";
            SubscriptionSettings settings;

            var builder = (SubscriptionBuilder) new SubscriptionBuilder(readQueue)
                .MaxAttemptsOnFailure(5);

            settings = builder.settings;

            Assert.AreEqual(false, settings.PauseOnError, "PauseOnError");

        }

        [TestMethod]
        public void SetupWithHandlers()
        {

            string readQueue = "myqueue";
            SubscriptionSettings settings;

            var builder = (SubscriptionBuilder) new SubscriptionBuilder(readQueue)
                .SetMessageHandler<string>((s) => System.Diagnostics.Debug.Write($"Handling: {s}"));

            settings = builder.settings;

            Type t1 = settings.MessageHandler.GetType().GetGenericArguments()[0];

            Assert.IsTrue(t1 == typeof(string), "MessageHandler: string");

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetupWithInvalidErrorHandlingSettings()
        {

            string readQueue = "myqueue";

            var builder = (SubscriptionBuilder) new SubscriptionBuilder(readQueue)
                .PauseAfterFailedAttempts(5);

            builder.settings.Validate();

        }

    }
}
