using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.Message.Actuator;

namespace Carebed.Infrastructure.Message.ActuatorMessages
{
    public class ActuatorStatusMessage : ActuatorMessageBase
    {
        /// <summary>
        /// A human-readable status message describing the actuator's current condition or transition.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The current or relevant state of the actuator.
        /// </summary>
        public required ActuatorState CurrentState { get; set; }
    }
}
