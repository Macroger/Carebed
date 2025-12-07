using System;
using System.IO;
using System.Threading.Tasks;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Logging;
using Carebed.Infrastructure.Message.SensorMessages;
using Carebed.Infrastructure.MessageEnvelope;
using Carebed.Infrastructure.Enums;
using Carebed.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carebed.Tests.Integration
{
 [TestClass]
 public class EventBusLoggerIntegrationTests
 {
 private string _tempDir = string.Empty;
 private string _logFile = "integration_log.txt";
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
 public async Task BasicEventBus_PublishedMessage_IsLoggedToFile_By_LoggingManager()
 {
 // Arrange
 var eventBus = new BasicEventBus();
 var logger = new SimpleFileLogger(_logPath);
 var loggingManager = new LoggingManager(_tempDir, _logFile, logger, eventBus);

 loggingManager.Start(); // subscribes to global messages and starts logger

 var sensorData = new SensorData
 {
 Value =12.34,
 Source = "int_sensor",
 SensorType = SensorTypes.Temperature,
 IsCritical = false
 };

 var payload = new SensorTelemetryMessage
 {
 SensorID = "int_sensor_1",
 TypeOfSensor = SensorTypes.Temperature,
 Data = sensorData
 };

 var envelope = new MessageEnvelope<SensorTelemetryMessage>(payload, MessageOrigins.SensorManager, MessageTypes.SensorData);

 // Act
 await eventBus.PublishAsync(envelope);

 // Allow background logger to process
 await Task.Delay(500);

 loggingManager.Stop();

 // Assert
 Assert.IsTrue(File.Exists(_logPath), "Log file was not created.");
 var content = File.ReadAllText(_logPath);
 Assert.IsTrue(content.Contains("int_sensor_1") || content.Contains("int_sensor"), "Log file does not contain expected sensor info.");
 }
 }
}
