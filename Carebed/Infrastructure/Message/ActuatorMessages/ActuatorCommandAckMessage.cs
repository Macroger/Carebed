using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.Message.Actuator;

namespace Carebed.Infrastructure.Message.ActuatorMessages
{
    public class ActuatorCommandAckMessage: ActuatorMessageBase
    {
        /// <summary>
        /// Represents the type of command that was acknowledged.
        /// </summary>
        public required ActuatorCommands CommandType { get; set; }

        /// <summary>
        /// Represents whether the actuator can execute the given command.
        /// </summary>
        public required bool CanExecuteCommand { get; set; }

        /// <summary>
        /// Represents an optional reason for the command acknowledgment. 
        /// Usually used with failed acknowledgments to provide context.
        /// </summary>
        public string? Reason { get; set; } = null;
    }
}
