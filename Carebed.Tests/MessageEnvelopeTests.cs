/******************************************************************************
 * File: MessageEnvelopeTests.cs
 * Project: Carebed.Tests
 * Description: Unit tests for MessageEnvelope<T> functionality.
 * 
 * Author: Mattthew Schatz
 * Date: November 8, 2025
 * 
 * C# Version: 13.0
 * .NET Target: .NET 8
 * 
 * Copilot AI Acknowledgement:
 *   Some or all of the tests in this file were generated or assisted by GitHub Copilot AI.
 *   Please review and validate for correctness and completeness.
 ******************************************************************************/
using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.Message.SensorMessages;
using Carebed.Infrastructure.MessageEnvelope;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace Carebed.Tests
{
    [TestClass]
    public class MessageEnvelopeTests
    {
        [TestMethod]
        public void Constructor_NullReferencePayload_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new MessageEnvelope<string>(null!, MessageOrigins.SensorManager, MessageTypes.SensorData);
            });
        }

        [TestMethod]
        public void Constructor_ValueTypePayload_AllowsNonNull()
        {
            var envelope = new MessageEnvelope<int>(42, MessageOrigins.EventBus, MessageTypes.Undefined);
            Assert.AreEqual(42, envelope.Payload);
        }

        [TestMethod]
        public void Properties_AreSet_Correctly_And_InterfacePayloadMatches()
        {
            var payload = "hello";
            var envelope = new MessageEnvelope<string>(payload, MessageOrigins.DisplayManager, MessageTypes.System);

            Assert.AreNotEqual(Guid.Empty, envelope.MessageId);
            Assert.AreEqual(MessageOrigins.DisplayManager, envelope.MessageOrigin);
            Assert.AreEqual(MessageTypes.System, envelope.MessageType);

            // Timestamp should be UTC and recent
            Assert.AreEqual(DateTimeKind.Utc, envelope.Timestamp.Kind);
            Assert.IsTrue((DateTime.UtcNow - envelope.Timestamp) < TimeSpan.FromSeconds(5), "Timestamp should be recent UTC");

            // Interface payload returns same instance
            IMessageEnvelope iface = envelope;
            Assert.AreSame(envelope.Payload, iface.Payload);
        }

        [TestMethod]
        public void ToString_Contains_Origin_Type_PayloadType_And_Id_And_IsoTimestamp()
        {
            var envelope = new MessageEnvelope<string>("p", MessageOrigins.AlertManager, MessageTypes.Alert);
            var s = envelope.ToString();

            Assert.IsTrue(s.Contains(envelope.MessageOrigin.ToString()), "ToString missing origin");
            Assert.IsTrue(s.Contains(envelope.MessageType.ToString()), "ToString missing type");
            Assert.IsTrue(s.Contains("PayloadType=String"), "ToString missing payload type");
            Assert.IsTrue(s.Contains("Id="), "ToString missing id");
            // ISO8601 "O" format contains 'T' and 'Z' for UTC
            Assert.IsTrue(s.Contains("T") && s.Contains("Z"), "ToString timestamp likely not ISO8601 'O' format");
        }

        private class DummyDisposable : IDisposable
        {
            public bool WasDisposed { get; private set; }
            public void Dispose() => WasDisposed = true;
        }

        [TestMethod]
        public void Dispose_Disposes_Payload_IfDisposable_And_Safe_To_Call_Twice()
        {
            var payload = new DummyDisposable();
            var envelope = new MessageEnvelope<DummyDisposable>(payload, MessageOrigins.SystemInitializer, MessageTypes.System);

            envelope.Dispose();
            Assert.IsTrue(payload.WasDisposed, "Disposable payload should have been disposed");

            // second call should not throw
            envelope.Dispose();
        }

        [TestMethod]
        public void Dispose_DoesNotThrow_For_NonDisposablePayload()
        {
            var envelope = new MessageEnvelope<int>(7, MessageOrigins.EventBus, MessageTypes.Undefined);
            envelope.Dispose(); // should not throw
        }

        [TestMethod]
        public void MessageId_IsUnique_AcrossInstances()
        {
            var a = new MessageEnvelope<string>("a", MessageOrigins.NetworkManager, MessageTypes.System);
            var b = new MessageEnvelope<string>("b", MessageOrigins.NetworkManager, MessageTypes.System);

            Assert.AreNotEqual(a.MessageId, b.MessageId);
        }
    }
}
