SimpleServiceBus
===========================

A messaging library for publish-subscribe implementation on top of MSMQ.
Supports transactional and non-transactional queues, dead letter queue and automatic retries with delay between failed attempts. 
Aims to provides a reliable yet very simple way to integrate applications running on the same machine or across networks.

Get it from Nuget:
```
    PM> Install-Package SimpleServiceBus 
```

Creating a Publisher
--------------------
In this example, the message (salesOrder) is sent to the "OrderQueue" if the transaction is completed successfully.


```csharp
    var salesOrder = new SalesOrder();

    var publisher = ServiceFactory
        .NewPublisher<SalesOrder>("OrderQueue")
        .AutoCreateLocalQueue()
        .UseAmbientTransaction()
        .Create();

    using (var scope = new TransactionScope())
    {
        ProcessOrder(salesOrder);
        publisher.Send(salesOrder);
        scope.Complete();
    }
```


Creating Subscriber
--------------------
Creating a subscriber is also this easy.
In this example, messages will be processed by a new instance of <code>OrderHandler</code> as they arrive in the OrderQueue.
If the message handler fails to process the message (by throwing an exception), the subscriber will try 4 consecutive times, pause for 2 minutes and try again. 
It will follow this pattern indefinitely until it is able to process the message. 
It only moves to the next message after successfully processing the current.

```csharp
    var subscriber = ServiceFactory.NewSubscription("OrderQueue")
        .AutoCreateLocalQueues()
        .MaxAttemptsOnFailure(4)
        .PauseAfterFailedAttempts(secondsToPause: 120)
        .SetMessageHandler<SalesOrder>((so) => new OrderHandler(so))
        .Create();

    subscriber.Start();
```

### Dead Letter Queue

This example is similar to the one above with one exception: instead of pausing and retrying the same message over and over, it will move the failed message to a dead letter and continue to process the next one.

```csharp
    var subscriber = ServiceFactory.NewSubscription("OrderQueue")
        .AutoCreateLocalQueues()
        .MaxAttemptsOnFailure(4)
        .WithErrorQueue("OrderProcessErrorQueue")
        .SetMessageHandler<SalesOrder>((so) => new OrderHandler(so))
        .Create();

    subscriber.Start();
```

### Stop On Error

Stops the subscriber after 4 failed attempts to process the same message. A new call to <code>Start</code> is required after the subscriber is stopped.

```csharp
    var subscriber = ServiceFactory.NewSubscription("OrderQueue")
        .AutoCreateLocalQueues()
        .MaxAttemptsOnFailure(4)
        .SetMessageHandler<SalesOrder>((so) => new OrderHandler(so))
        .Create();

    subscriber.Start();
```

Removing Messages From Queues
-----------------------------

By default, the subscriber will peek on the next message and send it to the message handler. Once the handler completes successfully (without throwing any exceptions) the message is removed from the queue.

To change this behavior when required and completely remove the message from the queue before it is passed to a message handler use the methods <code>DequeueBeforeHandling</code> while building a subscriber.

```csharp
    var subscriber = ServiceFactory.NewSubscription("OrderQueue")
        .AutoCreateLocalQueues()
        .MaxAttemptsOnFailure(4)
        .DequeueBeforeHandling()
        .SetMessageHandler<SalesOrder>((so) => new OrderHandler(so))                
        .Create();

    subscriber.Start();
```