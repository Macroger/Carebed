using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.MessageEnvelope;
using Carebed.Infrastructure.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Carebed.Tests
{
 [TestClass]
 public class EventBusTests
 {
 [TestMethod]
 public async Task MultipleGlobalSubscribers_AllReceive_PublishedMessage()
 {
 var bus = new BasicEventBus();
 int calls =0;

 Action<IMessageEnvelope> handler1 = _ => Interlocked.Increment(ref calls);
 Action<IMessageEnvelope> handler2 = _ => Interlocked.Increment(ref calls);

 bus.SubscribeToGlobalMessages(handler1);
 bus.SubscribeToGlobalMessages(handler2);

 var envelope = new MessageEnvelope<string>("x", MessageOrigins.EventBus, MessageTypes.System);
 await bus.PublishAsync(envelope);

 await Task.Delay(100); // allow handlers to run
 Assert.AreEqual(2, calls);

 // cleanup
 bus.UnsubscribeFromGlobalMessages(handler1);
 bus.UnsubscribeFromGlobalMessages(handler2);
 }

 [TestMethod]
 public async Task GlobalSubscriber_Receives_OnlyMatchingPayloadTypes_WhenChecked()
 {
 var bus = new BasicEventBus();
 bool gotString = false;
 bool gotInt = false;

 bus.SubscribeToGlobalMessages(env =>
 {
 if (env.Payload is string) gotString = true;
 if (env.Payload is int) gotInt = true;
 });

 await bus.PublishAsync(new MessageEnvelope<int>(123, MessageOrigins.EventBus, MessageTypes.Undefined));
 await bus.PublishAsync(new MessageEnvelope<string>("ok", MessageOrigins.EventBus, MessageTypes.Undefined));

 await Task.Delay(100);
 Assert.IsTrue(gotInt, "int payload should be received");
 Assert.IsTrue(gotString, "string payload should be received");
 }

 [TestMethod]
 public async Task UnsubscribeFromGlobalMessages_HandlerNotCalledAfterUnsubscribe()
 {
 var bus = new BasicEventBus();
 int called =0;
 Action<IMessageEnvelope> handler = _ => called++;

 bus.SubscribeToGlobalMessages(handler);
 await bus.PublishAsync(new MessageEnvelope<int>(1, MessageOrigins.EventBus, MessageTypes.Undefined));
 await Task.Delay(50);
 Assert.AreEqual(1, called);

 bus.UnsubscribeFromGlobalMessages(handler);
 await bus.PublishAsync(new MessageEnvelope<int>(2, MessageOrigins.EventBus, MessageTypes.Undefined));
 await Task.Delay(50);
 Assert.AreEqual(1, called, "Handler should not be invoked after unsubscribe");
 }

 [TestMethod]
 public async Task ExceptionInOneGlobalHandler_DoesNotPreventOthers()
 {
 var bus = new BasicEventBus();
 bool goodHandlerCalled = false;

 bus.SubscribeToGlobalMessages(_ => throw new InvalidOperationException("boom"));
 bus.SubscribeToGlobalMessages(_ => goodHandlerCalled = true);

 await bus.PublishAsync(new MessageEnvelope<string>("x", MessageOrigins.EventBus, MessageTypes.Undefined));
 await Task.Delay(100);

 Assert.IsTrue(goodHandlerCalled, "Other handlers should still be invoked when one throws.");
 }

 [TestMethod]
 public async Task ConcurrentPublishes_AllGlobalHandlersReceiveExpectedNumber()
 {
 var bus = new BasicEventBus();
 int counter =0;
 int publishCount =50;

 bus.SubscribeToGlobalMessages(_ => Interlocked.Increment(ref counter));

 var tasks = new Task[publishCount];
 for (int i =0; i < publishCount; i++)
 {
 tasks[i] = bus.PublishAsync(new MessageEnvelope<int>(i, MessageOrigins.EventBus, MessageTypes.Undefined));
 }

 await Task.WhenAll(tasks);
 await Task.Delay(200);

 Assert.AreEqual(publishCount, counter);
 }
 }
}
