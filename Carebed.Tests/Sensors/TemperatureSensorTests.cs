using System.Reflection;
using Carebed.Models.Sensors;
using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.Message.SensorMessages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Carebed.Tests.Sensors
{
    [TestClass]
    public class TemperatureSensorTests
    {
        [TestMethod]
        public void Constructor_SetsDefaults()
        {
            var sensor = new TemperatureSensor("temp1");
            Assert.AreEqual("temp1", sensor.SensorID);
            Assert.AreEqual(SensorTypes.Temperature, sensor.SensorType);
        }

        [TestMethod]
        public void ReadDataActual_ReturnsValidData()
        {
            var sensor = new TemperatureSensor("temp1");
            var data = sensor.ReadDataActual();
            Assert.IsNotNull(data);
            Assert.AreEqual("temp1", data.Source);
            Assert.AreEqual(SensorTypes.Temperature, data.SensorType);
            Assert.IsTrue(data.Value >= 35.0 && data.Value <= 40.0);
            Assert.IsNotNull(data.Metadata);
        }

        [TestMethod]
        public void ReadDataActual_CriticalDetection()
        {
            var sensor = new TemperatureSensor("temp1", min: 35.0, max: 40.0, criticalThreshold: 36.0);
            // Simulate multiple reads to check for critical
            bool foundCritical = false;
            for (int i = 0; i < 100; i++)
            {
                var data = sensor.ReadDataActual();
                if (data.Value > 36.0)
                {
                    Assert.IsTrue(data.IsCritical);
                    foundCritical = true;
                }
            }
            Assert.IsTrue(foundCritical, "Should detect at least one critical value in 100 samples.");
        }
    }
}
