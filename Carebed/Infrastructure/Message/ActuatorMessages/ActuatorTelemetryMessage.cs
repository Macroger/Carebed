using Carebed.Infrastructure.Message.Actuator;

namespace Carebed.Infrastructure.Message.ActuatorMessages
{
    public class ActuatorTelemetryMessage : ActuatorMessageBase
    {
        /// <summary>
        /// The physical position or setting of the actuator (e.g., "Raised", "Flat", "45°").
        /// </summary>
        public ActuatorPosition? Position { get; set; }

        /// <summary>
        /// The mechanical or electrical load currently being exerted (e.g., in Newtons or kilograms).
        /// </summary>
        public double? Load { get; set; }

        /// <summary>
        /// Optional temperature reading, if the actuator supports thermal monitoring.
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Optional error code or diagnostic flag, if the actuator is in a fault state.
        /// </summary>
        public required string ErrorCode { get; set; } = string.Empty;
    }
}
