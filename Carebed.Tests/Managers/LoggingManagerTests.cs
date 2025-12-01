using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Logging;
using Carebed.Infrastructure.Message.LoggerMessages;
using Carebed.Infrastructure.MessageEnvelope;
using Carebed.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Carebed.Tests.Managers
{
    [TestClass]
    public class LoggingManagerTests
    {
        private Mock<IFileLoggingService> _mockLoggingService;
        private Mock<IEventBus> _mockEventBus;
        private LoggingManager _manager;

        [TestInitialize]
        public void Setup()
        {
            _mockLoggingService = new Mock<IFileLoggingService>();
            _mockEventBus = new Mock<IEventBus>();
            _manager = new LoggingManager("logs", "log.txt", _mockLoggingService.Object, _mockEventBus.Object);
        }

        [TestMethod]
        public void IsValidFilePath_ReturnsFalse_ForNullOrWhitespace()
        {
            Assert.IsFalse(LoggingManager.IsValidFilePath(null));
            Assert.IsFalse(LoggingManager.IsValidFilePath(""));
            Assert.IsFalse(LoggingManager.IsValidFilePath("   "));
        }

        [TestMethod]
        public void IsValidFilePath_ReturnsFalse_ForInvalidChars()
        {
            var invalid = "invalid|file.txt";
            Assert.IsFalse(LoggingManager.IsValidFilePath(invalid));
        }

        [TestMethod]
        public void IsValidFilePath_ReturnsTrue_ForValidPath()
        {
            var valid = "validfile.txt";
            Assert.IsTrue(LoggingManager.IsValidFilePath(valid));
        }

        [TestMethod]
        public void UpdateLogLocation_CreatesDirectoryAndChangesFilePath()
        {
            var dir = "testlogs";
            var file = "testlog.txt";
            _mockLoggingService.Setup(s => s.ChangeFilePath(It.IsAny<string>())).Returns(true);

            var result = _manager.UpdateLogLocation(dir, file);

            Assert.IsTrue(result);
            _mockLoggingService.Verify(s => s.ChangeFilePath(System.IO.Path.Combine(dir, file)), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UpdateLogLocation_Throws_ForInvalidFilePath()
        {
            _manager.UpdateLogLocation("logs", "invalid|file.txt");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateLogLocation_Throws_WhenLoggerStarted()
        {
            // Simulate logger started
            _mockLoggingService.Setup(s => s.ChangeFilePath(It.IsAny<string>())).Returns(true);
            _manager.Start();
            _manager.UpdateLogLocation("logs", "log.txt");
        }

        [TestMethod]
        public void HandleLogMessage_DelegatesToLoggingService()
        {
            var envelope = new Mock<IMessageEnvelope>().Object;
            _manager.HandleLogMessage(envelope);
            _mockLoggingService.Verify(s => s.Log(envelope), Times.Once);
        }

        [TestMethod]
        public void Start_CallsLoggingServiceStart()
        {
            _mockLoggingService.Setup(s => s.Start()).Returns(Task.CompletedTask);
            _manager.Start();
            _mockLoggingService.Verify(s => s.Start(), Times.Once);
        }

        [TestMethod]
        public void Stop_CallsLoggingServiceStop()
        {
            _mockLoggingService.Setup(s => s.Stop()).Returns(Task.CompletedTask);
            _manager.Start();
            _manager.Stop();
            _mockLoggingService.Verify(s => s.Stop(), Times.Once);
        }

        [TestMethod]
        public void Dispose_CallsLoggingServiceDispose()
        {
            _mockLoggingService.Setup(s => s.Dispose());
            _manager.Dispose();
            _mockLoggingService.Verify(s => s.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task HandleLogCommand_StartCommand_InvokesStart()
        {
            var envelope = new MessageEnvelope<LoggerCommandMessage>(
                new LoggerCommandMessage(LoggerCommands.Start),
                MessageOrigins.LoggingManager,
                MessageTypes.LoggerCommandResponse);

            _mockLoggingService.Setup(s => s.Start()).Returns(Task.CompletedTask);
            _mockEventBus.Setup(b => b.PublishAsync(It.IsAny<MessageEnvelope<LoggerCommandAckMessage>>()))
                .Returns(Task.CompletedTask);

            var handleLogCommand = typeof(LoggingManager)
                .GetMethod("HandleLogCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task)handleLogCommand.Invoke(_manager, new object[] { envelope });
            await task;

            _mockLoggingService.Verify(s => s.Start(), Times.Once);
            _mockEventBus.Verify(b => b.PublishAsync(It.IsAny<MessageEnvelope<LoggerCommandAckMessage>>()), Times.Once);
        }

        [TestMethod]
        public async Task HandleLogCommand_StopCommand_InvokesStop()
        {
            // Start first
            _mockLoggingService.Setup(s => s.Start()).Returns(Task.CompletedTask);
            _manager.Start();

            var envelope = new MessageEnvelope<LoggerCommandMessage>(
                new LoggerCommandMessage(LoggerCommands.Stop),
                MessageOrigins.LoggingManager,
                MessageTypes.LoggerCommandResponse);

            _mockLoggingService.Setup(s => s.Stop()).Returns(Task.CompletedTask);
            _mockEventBus.Setup(b => b.PublishAsync(It.IsAny<MessageEnvelope<LoggerCommandAckMessage>>()))
                .Returns(Task.CompletedTask);

            var handleLogCommand = typeof(LoggingManager)
                .GetMethod("HandleLogCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task)handleLogCommand.Invoke(_manager, new object[] { envelope });
            await task;

            _mockLoggingService.Verify(s => s.Stop(), Times.Once);
            _mockEventBus.Verify(b => b.PublishAsync(It.IsAny<MessageEnvelope<LoggerCommandAckMessage>>()), Times.Once);
        }

        [TestMethod]
        public async Task HandleLogCommand_AdjustFilePath_InvokesUpdateLogLocation()
        {
            var metadata = new Dictionary<string, string>
            {
                { "LogDirectory", "newdir" },
                { "FilePath", "newfile.txt" }
            };
            var envelope = new MessageEnvelope<LoggerCommandMessage>(
                new LoggerCommandMessage(LoggerCommands.AdjustFilePath, metadata),
                MessageOrigins.LoggingManager,
                MessageTypes.LoggerCommandResponse);

            _mockLoggingService.Setup(s => s.ChangeFilePath(It.IsAny<string>())).Returns(true);
            _mockEventBus.Setup(b => b.PublishAsync(It.IsAny<MessageEnvelope<LoggerCommandAckMessage>>()))
                .Returns(Task.CompletedTask);

            var handleLogCommand = typeof(LoggingManager)
                .GetMethod("HandleLogCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task)handleLogCommand.Invoke(_manager, new object[] { envelope });
            await task;

            _mockLoggingService.Verify(s => s.ChangeFilePath(System.IO.Path.Combine("newdir", "newfile.txt")), Times.Once);
            _mockEventBus.Verify(b => b.PublishAsync(It.IsAny<MessageEnvelope<LoggerCommandAckMessage>>()), Times.Once);
        }

        [TestMethod]
        public async Task HandleLogCommand_StartCommand_SendsAckMessage()
        {
            var envelope = new MessageEnvelope<LoggerCommandMessage>(
                new LoggerCommandMessage(LoggerCommands.Start),
                MessageOrigins.LoggingManager,
                MessageTypes.LoggerCommandResponse);

            _mockLoggingService.Setup(s => s.Start()).Returns(Task.CompletedTask);
            _mockEventBus.Setup(b => b.PublishAsync(It.IsAny<MessageEnvelope<LoggerCommandAckMessage>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var handleLogCommand = typeof(LoggingManager)
                .GetMethod("HandleLogCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task)handleLogCommand.Invoke(_manager, new object[] { envelope });
            await task;

            _mockEventBus.Verify(b => b.PublishAsync(It.Is<MessageEnvelope<LoggerCommandAckMessage>>(ack =>
                ack.Payload.CommandType == LoggerCommands.Start)), Times.Once);
        }

        [TestMethod]
        public async Task HandleLogCommand_StopCommand_SendsAckMessage()
        {
            _mockLoggingService.Setup(s => s.Start()).Returns(Task.CompletedTask);
            _manager.Start();

            var envelope = new MessageEnvelope<LoggerCommandMessage>(
                new LoggerCommandMessage(LoggerCommands.Stop),
                MessageOrigins.LoggingManager,
                MessageTypes.LoggerCommandResponse);

            _mockLoggingService.Setup(s => s.Stop()).Returns(Task.CompletedTask);
            _mockEventBus.Setup(b => b.PublishAsync(It.IsAny<MessageEnvelope<LoggerCommandAckMessage>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var handleLogCommand = typeof(LoggingManager)
                .GetMethod("HandleLogCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task)handleLogCommand.Invoke(_manager, new object[] { envelope });
            await task;

            _mockEventBus.Verify(b => b.PublishAsync(It.Is<MessageEnvelope<LoggerCommandAckMessage>>(ack =>
                ack.Payload.CommandType == LoggerCommands.Stop)), Times.Once);
        }
    }
}
