namespace Carebed.Infrastructure.Message.SensorMessages
{
    public class SensorTelemetryMessage: SensorMessageBase
    {
        public required SensorData<object> Payload { get; set; }
    }
}
