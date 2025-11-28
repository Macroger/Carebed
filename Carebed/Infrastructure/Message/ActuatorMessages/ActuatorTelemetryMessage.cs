using Carebed.Infrastructure.Message.Actuator;

namespace Carebed.Infrastructure.Message.ActuatorMessages
{
    public class ActuatorTelemetryMessage : ActuatorMessageBase
    {
        /// <summary>
        /// The physical position or setting of the actuator (e.g., "Raised", "Flat", "45°").
        /// </summary>
        public ActuatorPosition? Position { get; init; }

        /// <summary>
        /// The mechanical or electrical load currently being exerted (e.g., in Newtons or kilograms).
        /// </summary>
        public double? Load { get; init; }

        /// <summary>
        /// Optional temperature reading, if the actuator supports thermal monitoring.
        /// </summary>
        public double? Temperature { get; init; }

        /// <summary>
        /// Optional error code or diagnostic flag, if the actuator is in a fault state.
        /// </summary>
        public required string ErrorCode { get; init; } = string.Empty;

        /// <summary>
        /// An optional reading for power consumption in watts, if applicable. 
        /// </summary>
        public double? Watts { get; init; }
    }
}
