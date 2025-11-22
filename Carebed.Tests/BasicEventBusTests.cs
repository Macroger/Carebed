using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Message;
using Carebed.Infrastructure.Message.SensorMessages;
using Carebed.Infrastructure.MessageEnvelope;

namespace Carebed.Tests.EventBus
{
    [TestClass]
    public class BasicEventBusTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task SubscribedHandler_ReceivesPublishedMessage()
        {
            var eventBus = new BasicEventBus();
            var received = false;
            SensorData sensorData = new SensorData
            {
                Value = 25.0,
                Source = "sensor1",
                SensorType = SensorTypes.HeartRate,
                IsCritical = false
            };

            eventBus.Subscribe<SensorTelemetryMessage>(envelope =>
            {
                received = true;
                Assert.AreEqual("sensor1", envelope.Payload.SensorID);
            });

            var message = new SensorTelemetryMessage { SensorID = "sensor1", TypeOfSensor = SensorTypes.Temperature, Data = sensorData};
            var envelope = new MessageEnvelope<SensorTelemetryMessage>(message, MessageOrigins.SensorManager, MessageTypes.SensorData);

            await eventBus.PublishAsync(envelope);

            Assert.IsTrue(received);
        }

        [TestMethod]
        public async Task MessagePublished_Event_Is_Raised_For_Any_Message()
        {
            // Arrange
            AbstractEventBus eventBus = new BasicEventBus();
            bool eventRaised = false;
            IMessageEnvelope? receivedEnvelope = null;

            TestContext.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Test line - I AM ALIVE!!!");

            Action<IMessageEnvelope> handler = (msg) =>
            {
                TestContext.WriteLine("Handler invoked!");
                eventRaised = true;
                receivedEnvelope = msg;
            };

            // Subscribe to the MessagePublished event - capture the envelope when raised
            eventBus.SubscribeToGlobalMessages(handler);

            var sensorData = new SensorData
            {
                Value = 99.9,
                Source = "sensorX",
                SensorType = SensorTypes.BloodOxygen,
                IsCritical = false
            };
            var message = new SensorTelemetryMessage
            {
                SensorID = "sensorX",
                TypeOfSensor = SensorTypes.BloodOxygen,
                Data = sensorData
            };

            var envelope = new MessageEnvelope<SensorTelemetryMessage>(
                message, MessageOrigins.SensorManager, MessageTypes.SensorData);

            // Act
            await eventBus.PublishAsync(envelope);

            // Assert
            Assert.IsTrue(eventRaised, "MessagePublished event was not raised.");
            Assert.IsNotNull(receivedEnvelope, "No envelope was received by the event handler.");
            Assert.AreEqual(envelope, receivedEnvelope, "The received envelope does not match the published envelope.");
        }
        

    }
}
