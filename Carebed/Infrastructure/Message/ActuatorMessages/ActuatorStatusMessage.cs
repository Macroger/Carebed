using Carebed.Infrastructure.Message.Actuator;

namespace Carebed.Infrastructure.Message.ActuatorMessages
{
    public class ActuatorStatusMessage : ActuatorMessageBase
    {
        /// <summary>
        /// A human-readable status message describing the actuator's current condition or transition.
        /// </summary>
        public required string Message { get; set; }
    }
}
