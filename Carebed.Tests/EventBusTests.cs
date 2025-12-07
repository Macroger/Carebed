using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.MessageEnvelope;
using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.Message.SensorMessages;
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

 var payload = new SensorTelemetryMessage
 {
 SensorID = "s1",
 TypeOfSensor = SensorTypes.Temperature,
 Data = new SensorData { Value =1.0, Source = "s1", SensorType = SensorTypes.Temperature, IsCritical = false }
 };
 var envelope = new MessageEnvelope<SensorTelemetryMessage>(payload, MessageOrigins.EventBus, MessageTypes.SensorData);
 await bus.PublishAsync(envelope);

 await Task.Delay(100); // allow handlers to run
 Assert.AreEqual(2, calls);

 // cleanup
 bus.UnsubscribeFromGlobalMessages(handler1);
 bus.UnsubscribeFromGlobalMessages(handler2);
 }

 [TestMethod]
 public async Task GenericSubscribe_ReceivesOnlyMatchingMessageType()
 {
 var bus = new BasicEventBus();
 bool receivedTelemetry = false;
 bool receivedStatus = false;

 bus.Subscribe<SensorTelemetryMessage>(envelope => receivedTelemetry = true);
 bus.Subscribe<SensorStatusMessage>(envelope => receivedStatus = true);

 // Publish a status message - should only trigger status handler
 var status = new SensorStatusMessage { SensorID = "sX", TypeOfSensor = SensorTypes.Temperature, CurrentState = SensorStates.Running };
 var envelopeStatus = new MessageEnvelope<SensorStatusMessage>(status, MessageOrigins.EventBus, MessageTypes.SensorStatus);
 await bus.PublishAsync(envelopeStatus);

 // Publish a telemetry message - should only trigger telemetry handler
 var telemetry = new SensorTelemetryMessage
 {
 SensorID = "sY",
 TypeOfSensor = SensorTypes.Temperature,
 Data = new SensorData { Value =2.0, Source = "sY", SensorType = SensorTypes.Temperature, IsCritical = false }
 };
 var envelopeTelemetry = new MessageEnvelope<SensorTelemetryMessage>(telemetry, MessageOrigins.EventBus, MessageTypes.SensorData);
 await bus.PublishAsync(envelopeTelemetry);

 await Task.Delay(100);

 Assert.IsTrue(receivedStatus);
 Assert.IsTrue(receivedTelemetry);
 }

 [TestMethod]
 public async Task UnsubscribeFromGlobalMessages_HandlerNotCalledAfterUnsubscribe()
 {
 var bus = new BasicEventBus();
 int called =0;
 Action<IMessageEnvelope> handler = _ => called++;

 bus.SubscribeToGlobalMessages(handler);
 await bus.PublishAsync(new MessageEnvelope<SensorTelemetryMessage>(new SensorTelemetryMessage { SensorID = "1", TypeOfSensor = SensorTypes.Temperature, Data = new SensorData { Value =1, Source = "1", SensorType = SensorTypes.Temperature, IsCritical = false } }, MessageOrigins.EventBus, MessageTypes.SensorData));
 await Task.Delay(50);
 Assert.AreEqual(1, called);

 bus.UnsubscribeFromGlobalMessages(handler);
 await bus.PublishAsync(new MessageEnvelope<SensorTelemetryMessage>(new SensorTelemetryMessage { SensorID = "2", TypeOfSensor = SensorTypes.Temperature, Data = new SensorData { Value =2, Source = "2", SensorType = SensorTypes.Temperature, IsCritical = false } }, MessageOrigins.EventBus, MessageTypes.SensorData));
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

 await bus.PublishAsync(new MessageEnvelope<SensorTelemetryMessage>(new SensorTelemetryMessage { SensorID = "x", TypeOfSensor = SensorTypes.Temperature, Data = new SensorData { Value =1, Source = "x", SensorType = SensorTypes.Temperature, IsCritical = false } }, MessageOrigins.EventBus, MessageTypes.SensorData));
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
 var msg = new SensorTelemetryMessage { SensorID = $"s{i}", TypeOfSensor = SensorTypes.Temperature, Data = new SensorData { Value = i, Source = $"s{i}", SensorType = SensorTypes.Temperature, IsCritical = false } };
 tasks[i] = bus.PublishAsync(new MessageEnvelope<SensorTelemetryMessage>(msg, MessageOrigins.EventBus, MessageTypes.SensorData));
 }

 await Task.WhenAll(tasks);
 await Task.Delay(200);

 Assert.AreEqual(publishCount, counter);
 }
 }
}
