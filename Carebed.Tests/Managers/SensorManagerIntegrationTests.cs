using Carebed.Models.Sensors;
using Carebed.Managers;
using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Message.SensorMessages;
using Carebed.Infrastructure.MessageEnvelope;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace Carebed.Tests.Managers
{
    [TestClass]
    public class SensorManagerIntegrationTests
    {
        [TestMethod]
        public async Task PollOnceAsync_PublishesTelemetry_ForRealSensors()
        {
            var eventBusMock = new Mock<IEventBus>();
            var publishedMessages = new List<object>();
            eventBusMock.Setup(x => x.PublishAsync(It.IsAny<MessageEnvelope<SensorTelemetryMessage>>()))
                .Callback<object>(msg => publishedMessages.Add(msg))
                .Returns(Task.CompletedTask);

            var sensors = new List<AbstractSensor>
            {
                new HeartRateSensor("hr1"),
                new TemperatureSensor("temp1")
            };
            var manager = new SensorManager(eventBusMock.Object, sensors, 100);

            var pollOnceAsyncMethod = typeof(SensorManager)
                .GetMethod("PollOnceAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            manager.Start();
            var task = pollOnceAsyncMethod?.Invoke(manager, null) as Task;
            await task;

            Assert.IsTrue(publishedMessages.Any(m => m is MessageEnvelope<SensorTelemetryMessage>));
            manager.Stop();
        }

        [TestMethod]
        public void StartStopSensors_ChangesState()
        {
            var eventBusMock = new Mock<IEventBus>();
            var sensors = new List<AbstractSensor>
            {
                new HeartRateSensor("hr1"),
                new TemperatureSensor("temp1")
            };
            var manager = new SensorManager(eventBusMock.Object, sensors, 100);

            manager.Start();
            foreach (var sensor in sensors)
            {
                Assert.AreEqual(SensorStates.Running, sensor.CurrentState);
            }
            manager.Stop();
            foreach (var sensor in sensors)
            {
                Assert.AreEqual(SensorStates.Stopped, sensor.CurrentState);
            }
        }
    }
}
