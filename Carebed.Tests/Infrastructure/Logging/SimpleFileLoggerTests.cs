using System;
using System.IO;
using System.Threading.Tasks;
using Carebed.Infrastructure.Logging;
using Carebed.Infrastructure.MessageEnvelope;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Carebed.Tests.Infrastructure.Logging
{
    [TestClass]
    public class SimpleFileLoggerTests
    {
        private string _tempFile;

        [TestInitialize]
        public void Setup()
        {
            _tempFile = Path.GetTempFileName();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempFile))
                File.Delete(_tempFile);
        }

        [TestMethod]
        public async Task Log_WritesMessageToFile()
        {
            var logger = new SimpleFileLogger(_tempFile);
            await logger.Start();

            var envelopeMock = new Mock<IMessageEnvelope>();
            envelopeMock.Setup(e => e.MessageId).Returns(Guid.NewGuid());
            envelopeMock.Setup(e => e.Timestamp).Returns(DateTime.UtcNow);
            envelopeMock.Setup(e => e.MessageOrigin).Returns(0);
            envelopeMock.Setup(e => e.MessageType).Returns(0);
            envelopeMock.Setup(e => e.Payload).Returns("TestPayload");

            logger.Log(envelopeMock.Object);
            await Task.Delay(300); // Allow background worker to process

            await logger.Stop();

            var fileContent = File.ReadAllText(_tempFile);
            Assert.IsTrue(fileContent.Contains("TestPayload"));
        }
    }
}
