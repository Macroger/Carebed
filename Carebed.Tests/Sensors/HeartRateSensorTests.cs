using Carebed.Models.Sensors;
using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.Message.SensorMessages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace Carebed.Tests.Sensors
{
    [TestClass]
    public class HeartRateSensorTests
    {
        [TestMethod]
        public void Constructor_SetsDefaults()
        {
            var sensor = new HeartRateSensor("hr1");
            Assert.AreEqual("hr1", sensor.SensorID);
            Assert.AreEqual(SensorTypes.HeartRate, sensor.SensorType);
        }

        [TestMethod]
        public void ReadDataActual_ReturnsValidData()
        {
            var sensor = new HeartRateSensor("hr1");
            var data = sensor.ReadDataActual();
            Assert.IsNotNull(data);
            Assert.AreEqual("hr1", data.Source);
            Assert.AreEqual(SensorTypes.HeartRate, data.SensorType);
            Assert.IsTrue(data.Value >= 40 && data.Value <= 130);
            Assert.IsNotNull(data.Metadata);
        }

        [TestMethod]
        public void ReadDataActual_CriticalDetection()
        {
            var sensor = new HeartRateSensor("hr1", min: 40, max: 130, lowCritical: 40, highCritical: 120);
            // Simulate multiple reads to check for critical
            bool foundCritical = false;
            for (int i = 0; i < 100; i++)
            {
                var data = sensor.ReadDataActual();
                if (data.Value < 40 || data.Value > 120)
                {
                    Assert.IsTrue(data.IsCritical);
                    foundCritical = true;
                }
            }
            Assert.IsTrue(foundCritical, "Should detect at least one critical value in 100 samples.");
        }
    }
}
