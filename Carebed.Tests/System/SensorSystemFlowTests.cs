using System;
using Carebed.Infrastructure;
using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Message.SensorMessages;
using Carebed.Infrastructure.MessageEnvelope;
using Carebed.Managers;
using Carebed.Models.Sensors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Carebed.Tests.System
{
    [TestClass]
    public class SensorSystemFlowTests
    {
        [TestMethod]
        public async Task SystemInitializer_StartsManagers_AndPublishesTelemetry_Normally()
        {
            // Arrange
            var (eventBus, managers, dashboard) = SystemInitializer.Initialize();
            bool telemetryReceived = false;

            // Subcribe to telemetry messages to verify they are published
            eventBus.Subscribe<SensorTelemetryMessage>(envelope => telemetryReceived = true);

            // Wait a momment to allow sensor telemetry readings to come in
            await Task.Delay(3000); // Wait 3 seconds for telemetry to be published

            // Stop and dispose all managers to clean up resources
            foreach (var manager in managers)
            {
                manager.Stop();
                (manager as IDisposable)?.Dispose();
            }

            Assert.IsTrue(telemetryReceived, "Telemetry should be published to event bus.");
        }
    }
}