using Carebed.Infrastructure.Enums;

namespace Carebed.Infrastructure.Message
{
    public class AlertMessage<TPayload> : IEventMessage
    {
        /// <summary>
        /// Represents the source identifier associated with the alert, e.g. "SensorManager", "ActuatorManager", "AlertHandler".
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Represents the type of message, which is always Alert for this class.
        /// </summary>
        public MessageType MessageType { get; } = MessageType.Alert;

        /// <summary>
        /// Represents the alert text or description.
        /// </summary>
        public string AlertText { get; set; } = string.Empty;

        /// <summary>
        /// Represents whether the alert is critical.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Represents whether the alert is a warning.
        /// </summary>
        public bool IsWarning { get; set; } = false;

        /// <summary>
        /// Represents whether the alert has been acknowledged.
        /// </summary>
        public bool IsAcknowledged { get; set; } = false;

        /// <summary>
        /// Represents the timestamp when the alert was created.
        /// </summary>
        public DateTime CreatedAt { get; } = DateTime.Now;

        /// <summary>
        /// Represents the correlation ID to link related messages together.
        /// </summary>
        public Guid CorrelationId { get; } = Guid.NewGuid();

        /// <summary>
        /// Represents an optional dictionary for additional metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string>? Metadata { get; set; }


        /// <summary>
        /// Represents the payload of the alert message.
        /// </summary>
        public TPayload? Payload { get; set; } // Optional: for domain-specific data
    }
}
        
