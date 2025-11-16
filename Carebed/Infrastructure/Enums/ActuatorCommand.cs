using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carebed.Infrastructure.Enums
{
    public enum ActuatorCommand
    {
        /// <summary>
        /// Instructs the actuator to begin upward or forward motion.
        /// </summary>
        Raise,

        /// <summary>
        /// Instructs the actuator to begin downward or backward motion.
        /// </summary>
        Lower,

        /// <summary>
        /// Requests the actuator to stop its current motion immediately.
        /// </summary>
        Stop,

        /// <summary>
        /// Locks the actuator to prevent further motion or commands.
        /// </summary>
        Lock,

        /// <summary>
        /// Unlocks the actuator to allow motion or command execution.
        /// </summary>
        Unlock,

        /// <summary>
        /// Resets the actuator to its default or idle state.
        /// </summary>
        Reset
    }
}
