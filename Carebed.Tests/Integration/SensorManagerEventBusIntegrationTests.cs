using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Logging;
using Carebed.Infrastructure.Message.SensorMessages;
using Carebed.Infrastructure.MessageEnvelope;
using Carebed.Infrastructure.Enums;
using Carebed.Managers;
using Carebed.Models.Sensors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carebed.Tests.Integration
{
 [TestClass]
 public class SensorManagerEventBusIntegrationTests
 {
 private string _tempDir = string.Empty;
 private string _logFile = "sensor_manager_integration_log.txt";
 private string _logPath = string.Empty;

 [TestInitialize]
 public void Setup()
 {
 _tempDir = Path.Combine(Path.GetTempPath(), "Carebed_IntegrationTests", Guid.NewGuid().ToString());
 Directory.CreateDirectory(_tempDir);
 _logPath = Path.Combine(_tempDir, _logFile);
 }

 [TestCleanup]
 public void Cleanup()
 {
 try
 {
 if (File.Exists(_logPath)) File.Delete(_logPath);
 if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
 }
 catch { }
 }

 [TestMethod]
 public async Task SensorManager_PublishesTelemetry_And_LoggingManager_WritesIt()
 {
 // Arrange
 var eventBus = new BasicEventBus();
 var logger = new SimpleFileLogger(_logPath);
 var loggingManager = new LoggingManager(_tempDir, _logFile, logger, eventBus);

 // Create a simple test sensor
 var sensors = new System.Collections.Generic.List<AbstractSensor>
 {
 new TestSensor("test_sensor_1", SensorTypes.Temperature)
 };

 var sensorManager = new SensorManager(eventBus, sensors,1000);

 loggingManager.Start();

 // Start sensor manager which should start sensors and emit telemetry when polled
 sensorManager.Start();

 // Wait enough time for a poll to occur and logging to process
 await Task.Delay(1500);

 sensorManager.Stop();
 loggingManager.Stop();

 // Assert that log file was created and contains telemetry data
 Assert.IsTrue(File.Exists(_logPath));
 var content = File.ReadAllText(_logPath);
 Assert.IsTrue(content.Contains("test_sensor_1") || content.Contains("test_sensor"));
 }

 private class TestSensor : AbstractSensor
 {
 public TestSensor(string id, SensorTypes type) : base(id, type,0,100,90)
 {
 }

 public override SensorData ReadDataActual()
 {
 return new SensorData
 {
 Value =1.23,
 Source = SensorID,
 SensorType = SensorType,
 IsCritical = false,
 CreatedAt = DateTime.UtcNow,
 CorrelationId = Guid.NewGuid()
 };
 }
 }
 }
}
