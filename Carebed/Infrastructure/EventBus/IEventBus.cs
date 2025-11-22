using Carebed.Infrastructure.Message;
using Carebed.Infrastructure.MessageEnvelope;

namespace Carebed.Infrastructure.EventBus
{
    /// <summary>
    /// Defines the contract for an event bus that supports subscribing,
    /// unsubscribing, publishing, and lifecycle management of event messages.
    /// </summary>
    public interface IEventBus
    {
        // Subscribe to all published messages (global notification)
        void SubscribeToGlobalMessages(Action<IMessageEnvelope> handler);

        void UnsubscribeFromGlobalMessages(Action<IMessageEnvelope> handler);

        /// <summary>
        /// Subscribes a handler to a specific event type.
        /// </summary>
        /// <typeparam name="TPayload">
        /// The type of the event message, constrained to <see cref="IEventMessage"/>.
        /// </typeparam>
        /// <param name="handler">The delegate to invoke when an event of type <typeparamref name="TPayload"/> is published.</param>
        void Subscribe<TPayload>(Action<MessageEnvelope<TPayload>> handler) where TPayload : IEventMessage;

        /// <summary>
        /// Unsubscribes a handler from a specific event type.
        /// </summary>
        /// <typeparam name="TPayload">
        /// The type of the event message, constrained to <see cref="IEventMessage"/>.
        /// </typeparam>
        /// <param name="handler">The delegate to remove from the subscriber registry.</param>
        void Unsubscribe<TPayload>(Action<MessageEnvelope<TPayload>> handler) where TPayload : IEventMessage;

        /// <summary>
        /// Publishes an event message asynchronously to all subscribed handlers.
        /// </summary>
        /// <typeparam name="TPayload">
        /// The type of the event message, constrained to <see cref="IEventMessage"/>.
        /// </typeparam>
        /// <param name="message">The event message instance to publish.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous publish operation.
        /// </returns>
        Task PublishAsync<TPayload>(MessageEnvelope<TPayload> message) where TPayload : IEventMessage;

        /// <summary>
        /// Initializes the event bus.
        /// </summary>
        /// <remarks>
        /// Intended for setup tasks such as preloading subscriptions or configuring tracing hooks.
        /// </remarks>
        void Initialize();

        /// <summary>
        /// Shuts down the event bus and clears all subscriptions.
        /// </summary>
        /// <remarks>
        /// After calling this method, no handlers will remain registered.
        /// </remarks>
        void Shutdown();
    }
}


