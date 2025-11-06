using Carebed.Infrastructure.MessageEnvelope;

namespace Carebed.Infrastructure.EventBus
{
    /// <summary>
    /// A bare-bones in-memory implementation of <see cref="AbstractEventBus"/>.
    /// Provides synchronous, inline dispatch of event messages.
    /// Useful for quick testing and regression scenarios.
    /// </summary>
    public class BasicEventBus : AbstractEventBus
    {
        /// <summary>
        /// Subscribes a handler to a specific event type.
        /// </summary>
        public override void Subscribe<TPayload>(Action<MessageEnvelope<TPayload>> handler)
        {
            base.Subscribe(handler);
        }

        /// <summary>
        /// Unsubscribes a handler from a specific event type.
        /// </summary>
        public override void Unsubscribe<TPayload>(Action<MessageEnvelope<TPayload>> handler)
        {
            base.Unsubscribe(handler);
        }

        /// <summary>
        /// Publishes an event message asynchronously to all subscribed handlers.
        /// </summary>
        public override async Task PublishAsync<TPayload>(MessageEnvelope<TPayload> message)
        {
            var handlers = GetHandlersFor<TPayload>();

            if (handlers == null || handlers.Count == 0)
            {
                return;
            }

            var tasks = new List<Task>(handlers.Count);

            foreach (var handler in handlers)
            {
                // run each handler on the thread-pool and isolate exceptions per-handler
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        handler(message);
                    }
                    catch (Exception ex)
                    {
                        // Keep failure of one handler from stopping others; surface to debug output
                        System.Diagnostics.Debug.WriteLine($"Event handler threw an exception: {ex}");
                    }
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes the event bus (no-op for BasicEventBus).
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Shuts down the event bus and clears all subscriptions.
        /// </summary>
        public override void Shutdown()
        {
            base.Shutdown();
        }
    }
}
