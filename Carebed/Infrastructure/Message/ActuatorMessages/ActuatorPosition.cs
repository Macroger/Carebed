using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carebed.Infrastructure.Message.ActuatorMessages
{
    public class ActuatorPosition
    {
        /// <summary>
        /// (Optional) Represents the linear extension of the actuator in units such as millimeters or inches.
        /// </summary>
        public double? Extension { get; set; }

        /// <summary>
        /// (Optional) Represents the angular position of the actuator in degrees.
        /// </summary>
        public double? Angle { get; set; }

        /// <summary>
        /// (Optional) Represents the X value of the 3D coordinates, if applicable.
        /// </summary>
        public double? X { get; set; }

        /// <summary>
        /// (Optional) Represents the Y value of the 3D coordinates, if applicable.
        /// </summary>
        public double? Y { get; set; }

        /// <summary>
        /// (Optional) Represents the Z value of the 3D coordinates, if applicable.
        /// </summary>
        public double? Z { get; set; }

        /// <summary>
        /// (Optional) Represents the raw or vendor-specific position string for debugging or fallback.
        /// </summary>
        public string? Raw { get; set; }
    }
}
