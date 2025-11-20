using Carebed.Infrastructure.Enums;

namespace Carebed.Infrastructure.Message.SensorMessages
{
    public class SensorStatusMessage: SensorMessageBase
    {
        /// <summary>
        /// A human-readable status message describing the sensor's current condition or transition.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The current or relevant state of the sensor.
        /// </summary>
        public required SensorStates CurrentState { get; set; }
    }
}
