using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Message.SensorMessages;
using Carebed.Infrastructure.MessageEnvelope;

namespace Carebed.Tests.EventBus
{
    [TestClass]
    public class BasicEventBusTests
    {
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
    }
}
