using Carebed.Infrastructure.Enums;

namespace Carebed.Infrastructure.Message.Actuator
{
    public abstract class ActuatorMessageBase : IEventMessage
    {
        /// <summary>
        /// The unique identifier of the actuator involved in this message.
        /// </summary>
        public required string ActuatorId { get; set; }

        /// <summary>
        /// The type of actuator (e.g., BedLift, HeadTilt).
        /// </summary>
        public required ActuatorTypes TypeOfActuator { get; set; }

        /// <summary>
        /// Timestamp of when the message was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// A required unique identifier to correlate related messages.
        /// </summary>
        public Guid CorrelationId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// An optional dictionary for additional metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// A flag indicating whether the message is critical.
        /// </summary>
        public bool IsCritical { get; set; } = false;

        /// <summary>
        /// A convenience method to provide a string representation of the actuator message.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GetType().Name}: ActuatorId={ActuatorId}, TypeOfActuator={TypeOfActuator}, CreatedAt={CreatedAt}, CorrelationId={CorrelationId}, IsCritical={IsCritical}";
        }
    }
}
