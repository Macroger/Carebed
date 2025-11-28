using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.Message.Actuator;

namespace Carebed.Infrastructure.Message.ActuatorMessages
{
    public class ActuatorErrorMessage : ActuatorMessageBase
    {
        /// <summary>
        /// A machine-readable error code representing the fault condition.
        /// Used for diagnostics, filtering, and automated responses.
        /// </summary>
        public required string ErrorCode { get; set; }

        /// <summary>
        /// A human-readable description of the error or fault condition.
        /// Intended for logs, UI alerts, and operator guidance.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// The current or relevant state of the actuator.
        /// </summary>
        public required ActuatorStates CurrentState { get; set; }
    }
}
