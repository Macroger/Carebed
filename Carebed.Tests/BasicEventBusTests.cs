using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Message;
using Carebed.Infrastructure.MessageEnvelope;

namespace Carebed.Tests.EventBus
{
    [TestClass]
    public class BasicEventBusTests
    {
        [TestMethod]
        public async Task PublishAsync_InvokesSubscribedHandler_WithEnvelope()
        {
            var bus = new BasicEventBus();

            // TaskCompletionSource to observe asynchronous handler invocation
            var tcs = new TaskCompletionSource<MessageEnvelope<SensorData>>();

            // Subscribe handler that receives the envelope
            bus.Subscribe<SensorData>(envelope =>
            {
                tcs.TrySetResult(envelope);
            });

            // Create payload and envelope
            var payload = new SensorData(42.5);
            var envelope = new MessageEnvelope<SensorData>(payload, MessageOriginEnum.SensorManager, MessageTypeEnum.SensorData);

            // Publish and await completion
            await bus.PublishAsync(envelope);

            // Handler should have received the envelope (awaiting tcs ensures stable assertion even if handler runs async)
            var received = await tcs.Task;
            Assert.AreSame(envelope, received);
            Assert.AreEqual(42.5, received.Payload.Value);
        }
    }
}
