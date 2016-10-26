using SimpleServiceBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SimpleServiceBus
{
    public class Subscriber : ISubscriber, IDisposable
    {

        private int maxFailedAttemps;
        private int failedAttemps;
        private bool onCompleteRegistered;
        private bool processing;
        private readonly SubscriptionSettings settings;
        private readonly ILogger log;       
        private readonly MessageQueue readQueue;
        private readonly MessageQueue errorQueue;
        private SubscriberStatus status;
        private readonly System.Timers.Timer pauseTimer;
        private AutoResetEvent readyToStop = new AutoResetEvent(false);

        internal Subscriber(SubscriptionSettings settings, MessageQueue readQueue, MessageQueue errorQueue,ILogger logger)
        {
            
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (readQueue == null) throw new ArgumentNullException(nameof(readQueue));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Trace($"{nameof(Subscriber)}() - [ctor]");

            status = SubscriberStatus.Stopped;
            this.settings = settings;
            this.readQueue = readQueue;
            this.errorQueue = errorQueue;
            this.log = logger;

            maxFailedAttemps = settings.AttemptsBeforeFail;

            if(settings.PauseOnError)
            {
                pauseTimer = new System.Timers.Timer(settings.SecondsToPause * 1000);
                pauseTimer.Elapsed += OnPauseTimerElapsedTime;
            }
            
        }

        public SubscriberStatus Status
        {
            get
            {
                return status;
            }
        }

        public void Start()
        {

            log.Trace($"{nameof(Start)}() started.");
            log.Info($"Attemting to start subscriber for queue: {readQueue.Path}");

            if (status != SubscriberStatus.Paused && status != SubscriberStatus.Stopped)
            {
                log.Error($"Attempt to start subscription failed. {nameof(Start)}() method was called when status was : {status.ToString()}.");
                throw new InvalidOperationException($"Subscriber cannot be started. Current state is: {status.ToString()}.");
            }

            try
            {

                status = SubscriberStatus.Starting;

                if(!onCompleteRegistered)
                {
                    log.Trace("Registering OnPeakComplete.");
                    readQueue.PeekCompleted += OnPeakComplete;
                    onCompleteRegistered = true;
                }
                else
                {
                    log.Trace("OnPeakComplete event already registered.");
                }

                readQueue.BeginPeek();
                status = SubscriberStatus.Started;

                log.Info($"Subscriber {readQueue.Path} started.");

            }
            catch (Exception ex)
            {
                status = SubscriberStatus.Stopped;
                log.Error($"Error while attemtping to start subscriber: {ex.Message}", ex);
                throw ex;
            }

            log.Trace($"{nameof(Start)}() complete.");

        }

        public void Stop()
        {

            log.Trace($"{nameof(Stop)}() started.");
            log.Info($"Attemting to stop subscriber for queue: {readQueue.Path}");

            status = SubscriberStatus.Stopping;
            if (processing)
            {
                readyToStop.WaitOne();
            }

            if (onCompleteRegistered)
            {
                readQueue.PeekCompleted -= OnPeakComplete;
                onCompleteRegistered = false;
            }

            status = SubscriberStatus.Stopped;
            log.Info($"Subscriber {readQueue.Path} stopped.");

            log.Trace($"{nameof(Stop)}() completed.");            

        }

        private void OnPeakComplete(object sender, PeekCompletedEventArgs e)
        {

            log.Trace($"{nameof(OnPeakComplete)} started.");

            bool ignoreError = false;

            if (status != SubscriberStatus.Started)
            {
                processing = false;
                log.Trace($"{nameof(OnPeakComplete)} complete without processing.");
                return;
            }

            Message message = null;
            processing = true;            

            try
            {

                message = readQueue.EndPeek(e.AsyncResult);

                if (settings.DequeueBeforeHandling)
                {                    
                    message = readQueue.Receive();
                }


                log.Trace($"Excuting handler on message id: {message.Id} - label: {message.Label}");

                settings.MessageHandler.Method.Invoke(settings.MessageHandler.Target, new object[] { message.Body });

                log.Trace("Handler execution complete.");

                if(!settings.DequeueBeforeHandling)
                {
                    log.Trace($"Removing message from queue: {message.Id} - label: {message.Label}");
                    readQueue.Receive();
                }

                // done with message
                log.Trace("");
                this.failedAttemps = 0;


            }
            catch (Exception ex)
            {

                failedAttemps++;

                if (settings.AttemptsBeforeFail > 1)
                {
                    
                    if(failedAttemps < settings.AttemptsBeforeFail)
                    {
                        log.Warn($"Error processing message. Attempt {failedAttemps}/{maxFailedAttemps}. Error: {GetMessageFromException(ex)}");
                        ignoreError = true;
                    }
                }

                if(!ignoreError)
                {
                    
                    if (settings.PauseOnError)
                    {
                        log.Warn($"Subscriber will pause message processing for {settings.SecondsToPause} seconds.");
                        status = SubscriberStatus.Paused;
                        pauseTimer.Enabled = true;
                    }
                    else
                    {

                        log.Error($"Error processing message. {failedAttemps}/{maxFailedAttemps} Failed attempts. Error{GetMessageFromException(ex)}", ex);

                        if (errorQueue == null)
                        {
                            log.Warn("Error queue is not set. Message can't be moved out of queue. Stopping subscriber.");
                            status = SubscriberStatus.Stopped;
                        }
                        else
                        {
                            MoveToErrorQueue(message);
                        }
                        
                    }

                    failedAttemps = 0;

                }
     
            }
            

            try
            {

                readQueue.Refresh();

                if (status == SubscriberStatus.Started & (pauseTimer == null || !pauseTimer.Enabled))
                {
                    readQueue.BeginPeek();
                }
                    

            }
            catch (Exception ex)
            {
                log.Error($"Error refreshing queue: {ex.Message}", ex);
            }

            processing = false;
            readyToStop.Set();

            log.Trace($"{nameof(OnPeakComplete)} complete.");

        }

        private void OnPauseTimerElapsedTime(object source, ElapsedEventArgs e)
        {

            pauseTimer.Enabled = false;
            processing = false;
            status = SubscriberStatus.Started;

            if(readQueue != null)
            {
                readQueue.BeginPeek();
            }

        }

        private bool MoveToErrorQueue(Message message)
        {

            log.Trace($"{nameof(MoveToErrorQueue)}() started.");

            bool retValue = false;

            if (message == null)
            {
                log.Trace($"{nameof(MoveToErrorQueue)}() complete. Message is null.");
                return true;
            }

            if (errorQueue == null)
            {
                log.Trace($"{nameof(MoveToErrorQueue)}() complete. Error queue not set.");
                return true;
            }

            try
            {


                var errorMessage = new Message(message.Body);
                if(!string.IsNullOrWhiteSpace(message.Label))
                {
                    errorMessage.Label = message.Label;
                }                

                log.Trace("Moving message to error queue.");
                if (errorQueue.Transactional)
                {
                    errorQueue.Send(errorMessage, MessageQueueTransactionType.Single);
                }
                else
                {
                    errorQueue.Send(errorMessage);
                }

                if (!settings.DequeueBeforeHandling)
                {
                    log.Trace($"Removing message from queue: {message.Id} - label: {message.Label}");
                    readQueue.Receive();
                }

                retValue = true;

            }
            catch (Exception ex)
            {
                log.Error($"Unable to move message id: {message.Id} - label: {message.Label}, to error queue., Error: {ex.Message}", ex);
            }

            log.Trace($"{nameof(MoveToErrorQueue)}() complete.");
            return retValue;

        }

        private string GetMessageFromException(Exception exception)
        {

            string message;

            if(exception.InnerException != null)
            {
                message = $"{exception.Message} - Inner Exception: {exception.InnerException.Message}";
            }
            else
            {
                message = exception.Message;
            }

            return message;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(pauseTimer != null)
                    {
                        pauseTimer.Elapsed -= OnPauseTimerElapsedTime;
                    }
                    if(readQueue != null && onCompleteRegistered)
                    {
                        readQueue.PeekCompleted -= OnPeakComplete;
                    }
                }
                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion

    }
}
